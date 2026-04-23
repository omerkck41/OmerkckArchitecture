namespace Kck.FileStorage.Abstractions;

/// <summary>
/// FTP-specific operations beyond basic storage.
/// </summary>
public interface IFtpService : IFileStorageService
{
    Task CreateDirectoryAsync(string path, CancellationToken ct = default);
    Task DeleteDirectoryAsync(string path, CancellationToken ct = default);
    Task MoveAsync(string sourcePath, string destinationPath, CancellationToken ct = default);
}
