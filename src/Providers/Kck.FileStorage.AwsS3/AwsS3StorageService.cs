using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Kck.FileStorage.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.FileStorage.AwsS3;

/// <summary>
/// <see cref="IFileStorageService"/> implementation backed by Amazon S3.
/// Supports explicit access-key credentials and IAM role / environment-variable authentication.
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
    public async Task UploadAsync(FileUploadRequest request, CancellationToken ct = default)
    {
        var key = BuildKey(request.DestinationPath, request.FileName);
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = request.Content,
            AutoCloseStream = false
        };
        await _s3.PutObjectAsync(putRequest, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
    {
        var response = await _s3.GetObjectAsync(_bucket, ApplyPrefix(path), ct).ConfigureAwait(false);
        return response.ResponseStream;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(_bucket, ApplyPrefix(path), ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
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
        var prefix = ApplyPrefix(directoryPath.TrimEnd('/') + "/");
        var request = new ListObjectsV2Request { BucketName = _bucket, Prefix = prefix };
        var results = new List<FileMetadata>();

        ListObjectsV2Response response;
        do
        {
            response = await _s3.ListObjectsV2Async(request, ct).ConfigureAwait(false);
            foreach (var obj in response.S3Objects)
            {
                results.Add(new FileMetadata
                {
                    FileName = Path.GetFileName(obj.Key),
                    FullPath = obj.Key,
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

    private string ApplyPrefix(string path) =>
        string.IsNullOrEmpty(_keyPrefix)
            ? path
            : $"{_keyPrefix.TrimEnd('/')}/{path.TrimStart('/')}";

    private static AmazonS3Client BuildClient(AwsS3Options opts)
    {
        var region = RegionEndpoint.GetBySystemName(opts.Region);

        if (!string.IsNullOrWhiteSpace(opts.AccessKey) && !string.IsNullOrWhiteSpace(opts.SecretKey))
        {
            var credentials = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
            return new AmazonS3Client(credentials, region);
        }

        // Fall through to IAM role / env var credentials (DefaultInstanceProfileAWSCredentials etc.)
        return new AmazonS3Client(region);
    }

    /// <inheritdoc/>
    public void Dispose() => _s3.Dispose();
}
