using Core.Infrastructure.UniversalFTP.Services.Models;

namespace Core.Infrastructure.UniversalFTP.Services.Interfaces;

public interface IFtpDirectoryService
{
    Task<FtpOperationResult> CreateDirectoryAsync(string remotePath);
    Task<FtpOperationResult> DeleteDirectoryAsync(string remotePath);
    Task<FtpOperationResult> MoveDirectoryAsync(string oldPath, string newPath);
    Task<IEnumerable<string>> ListFilesAsync(string remotePath);
    Task<bool> DirectoryExistsAsync(string remotePath);
}