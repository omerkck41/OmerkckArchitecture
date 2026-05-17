namespace Kck.FileStorage.Abstractions;

/// <summary>
/// FTP-specific operations beyond basic storage.
/// </summary>
public interface IFtpService : IFileStorageService
{
    /// <summary>
    /// Creates a directory at the specified path on the FTP server.
    /// </summary>
    Task CreateDirectoryAsync(string path, CancellationToken ct = default);
    /// <summary>
    /// Deletes the directory at the specified path from the FTP server.
    /// </summary>
    Task DeleteDirectoryAsync(string path, CancellationToken ct = default);
    /// <summary>
    /// Moves or renames a file from the source path to the destination path on the FTP server.
    /// </summary>
    Task MoveAsync(string sourcePath, string destinationPath, CancellationToken ct = default);
}
