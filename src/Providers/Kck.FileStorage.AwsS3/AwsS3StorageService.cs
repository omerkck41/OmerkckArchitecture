using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Kck.FileStorage.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.FileStorage.AwsS3;

/// <summary>
/// <see cref="IFileStorageService"/> implementation backed by Amazon S3.
/// Supports explicit access-key credentials and IAM role / environment-variable authentication. Every path argument
/// is checked with <see cref="PathHelper.Validate"/> before a request is built: a path containing <c>..</c>, an
/// absolute path, a percent sign or a control character is rejected with <see cref="ArgumentException"/> (the same
/// rule as the Blob and FTP providers; S3 keeps <c>..</c> literally in the key, so no object outside
/// <c>KeyPrefix</c> was reachable, but the key was still written). An empty <c>DestinationPath</c> or
/// <c>directoryPath</c> means the bucket root (under <c>KeyPrefix</c> when one is set).
/// </summary>
public sealed class AwsS3StorageService : IFileStorageService, IDisposable
{
    private readonly AmazonS3Client _s3;
    private readonly string _bucket;
    private readonly string? _keyPrefix;

    /// <summary>Initialises the service and builds the S3 client.</summary>
    public AwsS3StorageService(IOptionsMonitor<AwsS3Options> options)
    {
        var opts = options.CurrentValue;
        _bucket = opts.BucketName;
        _keyPrefix = opts.KeyPrefix;
        _s3 = BuildClient(opts);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// With <see cref="FileUploadRequest.Overwrite"/> set to <see langword="false"/> the object is written with a
    /// conditional request (<c>If-None-Match: *</c>), so an existing object is never replaced, also not by a
    /// concurrent writer. The service must support S3 conditional writes, as Amazon S3 does; an S3-compatible service
    /// that ignores the header overwrites the object. A 409 <c>ConditionalRequestConflict</c> (a concurrent conditional
    /// write to the same key) is reported the same way as an existing object.
    /// </remarks>
    /// <exception cref="IOException">
    /// <see cref="FileUploadRequest.Overwrite"/> is <see langword="false"/> and the object already exists, or a
    /// concurrent conditional write to the same key conflicted.
    /// </exception>
    public async Task UploadAsync(FileUploadRequest request, CancellationToken ct = default)
    {
        ValidateOptionalPath(request.DestinationPath);
        PathHelper.Validate(request.FileName);

        var key = BuildKey(request.DestinationPath, request.FileName);
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = request.Content,
            AutoCloseStream = false
        };
        if (!request.Overwrite)
            putRequest.IfNoneMatch = "*";

        try
        {
            await _s3.PutObjectAsync(putRequest, ct).ConfigureAwait(false);
        }
        catch (AmazonS3Exception ex) when (!request.Overwrite && IsExistingObjectConflict(ex))
        {
            throw new IOException($"The object '{key}' already exists and Overwrite is false.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
    {
        PathHelper.Validate(path);

        var response = await _s3.GetObjectAsync(_bucket, ApplyPrefix(path), ct).ConfigureAwait(false);
        return response.ResponseStream;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        PathHelper.Validate(path);

        await _s3.DeleteObjectAsync(_bucket, ApplyPrefix(path), ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        PathHelper.Validate(path);

        try
        {
            await _s3.GetObjectMetadataAsync(_bucket, ApplyPrefix(path), ct).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileMetadata>> ListAsync(string directoryPath, CancellationToken ct = default)
    {
        ValidateOptionalPath(directoryPath);

        // An empty directory is the bucket (or KeyPrefix) root: prefix "" or "<KeyPrefix>/", never "/".
        var directory = directoryPath.Trim('/');
        var prefix = ApplyPrefix(directory.Length == 0 ? "" : directory + "/"); // NOSONAR S1075 - S3 key delimiter is always '/', not platform-dependent
        var request = new ListObjectsV2Request { BucketName = _bucket, Prefix = prefix };
        var results = new List<FileMetadata>();

        ListObjectsV2Response response;
        do
        {
            response = await _s3.ListObjectsV2Async(request, ct).ConfigureAwait(false);
            // SDK v4 leaves S3Objects null when the page has no objects.
            foreach (var obj in response.S3Objects ?? [])
            {
                results.Add(new FileMetadata
                {
                    FileName = Path.GetFileName(obj.Key),
                    FullPath = RemovePrefix(obj.Key),
                    SizeInBytes = obj.Size ?? 0,
                    ModifiedAt = obj.LastModified
                });
            }
            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        return results.AsReadOnly();
    }

    private string BuildKey(string path, string fileName)
    {
        var dir = path.Trim('/');
        var relative = string.IsNullOrEmpty(dir) ? fileName : $"{dir}/{fileName}";
        return ApplyPrefix(relative);
    }

    // An empty destination or directory means the bucket (or KeyPrefix) root; any other path argument must be a relative path.
    private static void ValidateOptionalPath(string path)
    {
        if (!string.IsNullOrEmpty(path))
            PathHelper.Validate(path);
    }

    private string ApplyPrefix(string path) =>
        string.IsNullOrEmpty(_keyPrefix)
            ? path
            : $"{_keyPrefix.TrimEnd('/')}/{path.TrimStart('/')}";

    // The inverse of ApplyPrefix for listed keys: FullPath is the path a caller passes back to Download/Exists/Delete.
    private string RemovePrefix(string key)
    {
        var root = ApplyPrefix("");
        return root.Length > 0 && key.StartsWith(root, StringComparison.Ordinal) ? key[root.Length..] : key;
    }

    /// <summary>
    /// Whether a failed conditional upload (<c>If-None-Match: *</c>) means the object exists: 412 Precondition Failed,
    /// or 409 <c>ConditionalRequestConflict</c> when a concurrent conditional write to the same key won.
    /// </summary>
    internal static bool IsExistingObjectConflict(AmazonS3Exception ex) =>
        ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed
        || (ex.StatusCode == System.Net.HttpStatusCode.Conflict
            && string.Equals(ex.ErrorCode, "ConditionalRequestConflict", StringComparison.Ordinal));

    private static AmazonS3Client BuildClient(AwsS3Options opts)
    {
        var config = CreateClientConfig(opts);

        if (!string.IsNullOrWhiteSpace(opts.AccessKey) && !string.IsNullOrWhiteSpace(opts.SecretKey))
        {
            var credentials = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
            return new AmazonS3Client(credentials, config);
        }

        // Fall through to IAM role / env var credentials (DefaultInstanceProfileAWSCredentials etc.)
        return new AmazonS3Client(config);
    }

    internal static AmazonS3Config CreateClientConfig(AwsS3Options opts)
    {
        var config = new AmazonS3Config { ForcePathStyle = opts.ForcePathStyle };

        if (string.IsNullOrWhiteSpace(opts.ServiceUrl))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(opts.Region);
        }
        else
        {
            // ServiceURL and RegionEndpoint are mutually exclusive; the region still signs requests (SigV4).
            config.ServiceURL = opts.ServiceUrl;
            config.AuthenticationRegion = opts.Region;
        }

        if (opts.ChecksumWhenRequired)
        {
            config.RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED;
            config.ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED;
        }

        return config;
    }

    /// <inheritdoc/>
    public void Dispose() => _s3.Dispose();
}
