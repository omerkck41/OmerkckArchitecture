using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;
using Kck.FileStorage.Abstractions;
using Microsoft.Extensions.Options;

namespace Kck.FileStorage.AzureBlob;

/// <summary>
/// <see cref="IFileStorageService"/> implementation backed by Azure Blob Storage.
/// Supports connection-string and Managed Identity authentication.
/// </summary>
public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;

    /// <summary>
    /// Initialises the service and optionally ensures the container exists.
    /// </summary>
    public AzureBlobStorageService(IOptionsMonitor<AzureBlobOptions> options)
    {
        var opts = options.CurrentValue;
        _container = BuildContainerClient(opts);

        if (opts.CreateContainerIfNotExists)
            _container.CreateIfNotExists(PublicAccessType.None);
    }

    /// <inheritdoc/>
    public async Task UploadAsync(FileUploadRequest request, CancellationToken ct = default)
    {
        var blobName = BuildBlobName(request.DestinationPath, request.FileName);
        var blob = _container.GetBlobClient(blobName);
        await blob.UploadAsync(request.Content, overwrite: request.Overwrite, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(path);
        var response = await blob.DownloadStreamingAsync(cancellationToken: ct).ConfigureAwait(false);
        return response.Value.Content;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(path);
        await blob.DeleteIfExistsAsync(cancellationToken: ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(path);
        var response = await blob.ExistsAsync(ct).ConfigureAwait(false);
        return response.Value;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FileMetadata>> ListAsync(string directoryPath, CancellationToken ct = default)
    {
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
}
