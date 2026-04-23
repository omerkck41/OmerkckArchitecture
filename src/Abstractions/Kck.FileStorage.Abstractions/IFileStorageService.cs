namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Provider-agnostic file storage (FTP, local disk, cloud blob, etc.).
/// </summary>
public interface IFileStorageService
{
    Task UploadAsync(FileUploadRequest request, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
    Task<IReadOnlyList<FileMetadata>> ListAsync(string directoryPath, CancellationToken ct = default);
}
