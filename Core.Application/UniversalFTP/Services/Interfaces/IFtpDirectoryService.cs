using Core.Application.UniversalFTP.Services.Models;

namespace Core.Application.UniversalFTP.Services.Interfaces;

public interface IFtpDirectoryService
{
    Task<FtpOperationResult> CreateDirectoryAsync(string remotePath);
    Task<FtpOperationResult> DeleteDirectoryAsync(string remotePath);
    Task<FtpOperationResult> MoveDirectoryAsync(string oldPath, string newPath);
    Task<IEnumerable<string>> ListFilesAsync(string remotePath);
    Task<bool> DirectoryExistsAsync(string remotePath);
}