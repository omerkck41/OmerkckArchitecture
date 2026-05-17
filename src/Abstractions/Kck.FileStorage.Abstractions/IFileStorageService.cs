namespace Kck.FileStorage.Abstractions;

/// <summary>
/// Provider-agnostic file storage (FTP, local disk, cloud blob, etc.).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to the storage provider using the details in the specified request.
    /// </summary>
    Task UploadAsync(FileUploadRequest request, CancellationToken ct = default);
    /// <summary>
    /// Downloads the file at the specified path and returns its contents as a stream.
    /// </summary>
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    /// <summary>
    /// Deletes the file at the specified path from the storage provider.
    /// </summary>
    Task DeleteAsync(string path, CancellationToken ct = default);
    /// <summary>
    /// Returns true if a file exists at the specified path; otherwise false.
    /// </summary>
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
    /// <summary>
    /// Lists the metadata of all files in the specified directory path.
    /// </summary>
    Task<IReadOnlyList<FileMetadata>> ListAsync(string directoryPath, CancellationToken ct = default);
}
