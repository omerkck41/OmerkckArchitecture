using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;
using Kck.FileStorage.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.FileStorage.AzureBlob;

/// <summary>
/// <see cref="IFileStorageService"/> implementation backed by Azure Blob Storage.
/// Supports connection-string and Managed Identity authentication. Every path argument is checked with
/// <see cref="PathHelper.Validate"/> before a request is built: a path containing <c>..</c>, an absolute path, a
/// percent sign or a control character is rejected with <see cref="ArgumentException"/>, because the blob name becomes
/// part of the request URI and <c>..</c> would resolve to another container. An empty <c>DestinationPath</c> or
/// <c>directoryPath</c> means the container root.
/// With <see cref="AzureBlobOptions.CreateContainerIfNotExists"/> the container is created on first use, not at
/// construction: the first operation after path validation creates it asynchronously. Concurrent callers share one
/// attempt and, when it fails, all fail with its exception; a failed attempt is not cached and the next call retries.
/// </summary>
[SuppressMessage("Reliability", "CA1001:Types that own disposable fields should be disposable",
    Justification = "The SemaphoreSlim is only awaited (WaitAsync), so it never allocates a wait handle; it lives as long as the singleton service. Adding IDisposable would change the public type.")]
public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;
    private readonly bool _createContainerIfNotExists;
    private readonly SemaphoreSlim _containerLock = new(1, 1);
    private bool _containerEnsured;
    // The in-flight CreateIfNotExists attempt shared by concurrent callers; written under _containerLock.
    private Task? _containerCreation;

    /// <summary>
    /// Initialises the service. No request is sent here; the container is created on first use when
    /// <see cref="AzureBlobOptions.CreateContainerIfNotExists"/> is set.
    /// </summary>
    public AzureBlobStorageService(IOptionsMonitor<AzureBlobOptions> options)
    {
        var opts = options.CurrentValue;
        _container = BuildContainerClient(opts);
        _createContainerIfNotExists = opts.CreateContainerIfNotExists;
    }

    /// <inheritdoc/>
    public async Task UploadAsync(FileUploadRequest request, CancellationToken ct = default)
    {
        ValidateOptionalPath(request.DestinationPath);
        PathHelper.Validate(request.FileName);
        await EnsureContainerAsync(ct).ConfigureAwait(false);

        var blobName = BuildBlobName(request.DestinationPath, request.FileName);
        var blob = _container.GetBlobClient(blobName);
        await blob.UploadAsync(request.Content, overwrite: request.Overwrite, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
    {
        PathHelper.Validate(path);
        await EnsureContainerAsync(ct).ConfigureAwait(false);

        var blob = _container.GetBlobClient(path);
        var response = await blob.DownloadStreamingAsync(cancellationToken: ct).ConfigureAwait(false);
        return response.Value.Content;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        PathHelper.Validate(path);
        await EnsureContainerAsync(ct).ConfigureAwait(false);

        var blob = _container.GetBlobClient(path);
        await blob.DeleteIfExistsAsync(cancellationToken: ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        PathHelper.Validate(path);
        await EnsureContainerAsync(ct).ConfigureAwait(false);

        var blob = _container.GetBlobClient(path);
        var response = await blob.ExistsAsync(ct).ConfigureAwait(false);
        return response.Value;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileMetadata>> ListAsync(string directoryPath, CancellationToken ct = default)
    {
        ValidateOptionalPath(directoryPath);
        await EnsureContainerAsync(ct).ConfigureAwait(false);

        var prefix = directoryPath.TrimEnd('/') + "/";
        var results = new List<FileMetadata>();

        await foreach (var item in _container.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, prefix, ct).ConfigureAwait(false))
        {
            results.Add(new FileMetadata
            {
                FileName = Path.GetFileName(item.Name),
                FullPath = item.Name,
                SizeInBytes = item.Properties.ContentLength ?? 0,
                CreatedAt = item.Properties.CreatedOn,
                ModifiedAt = item.Properties.LastModified
            });
        }

        return results.AsReadOnly();
    }

    // Creates the container once, on first use. Concurrent first callers share one in-flight attempt, so an unreachable
    // account costs one SDK retry schedule rather than one per waiting caller; when it fails they all fail with its
    // exception. The flag is set only after success and a failed attempt is cleared, so the next call starts a new one.
    private async ValueTask EnsureContainerAsync(CancellationToken ct)
    {
        if (!_createContainerIfNotExists || Volatile.Read(ref _containerEnsured))
            return;

        Task attempt;
        await _containerLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_containerEnsured)
                return;

            attempt = _containerCreation ??= CreateContainerAsync();
        }
        finally
        {
            _containerLock.Release();
        }

        await attempt.WaitAsync(ct).ConfigureAwait(false);
    }

    // The shared attempt runs without a caller's token: one caller cancelling must not fail the others.
    private async Task CreateContainerAsync()
    {
        try
        {
            await _container.CreateIfNotExistsAsync(PublicAccessType.None).ConfigureAwait(false);
            Volatile.Write(ref _containerEnsured, true);
        }
        finally
        {
            await _containerLock.WaitAsync().ConfigureAwait(false);
            try
            {
                _containerCreation = null;
            }
            finally
            {
                _containerLock.Release();
            }
        }
    }

    private static BlobContainerClient BuildContainerClient(AzureBlobOptions opts)
    {
        if (!string.IsNullOrWhiteSpace(opts.ConnectionString))
            return new BlobContainerClient(opts.ConnectionString, opts.ContainerName);

        if (!string.IsNullOrWhiteSpace(opts.AccountName))
        {
            var uri = new Uri($"https://{opts.AccountName}.blob.core.windows.net/{opts.ContainerName}");
            return new BlobContainerClient(uri, new DefaultAzureCredential());
        }

        throw new InvalidOperationException(
            """
            [Kck.FileStorage.AzureBlob] AzureBlobOptions is invalid:
              Set either ConnectionString or AccountName (for Managed Identity).
              Example: opt.ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION");
            """);
    }

    private static string BuildBlobName(string path, string fileName)
    {
        var dir = path.Trim('/');
        return string.IsNullOrEmpty(dir) ? fileName : $"{dir}/{fileName}";
    }

    // An empty destination or directory means the container root; any other path argument must be a relative path.
    private static void ValidateOptionalPath(string path)
    {
        if (!string.IsNullOrEmpty(path))
            PathHelper.Validate(path);
    }
}
