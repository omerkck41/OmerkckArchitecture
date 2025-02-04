using Core.Infrastructure.UniversalFTP.Services.Models;

namespace Core.Infrastructure.UniversalFTP.Services.Interfaces;

public interface IFtpService
{
    Task<FtpOperationResult> UploadFileAsync(string localPath, string remotePath);
    Task<FtpOperationResult> DownloadFileAsync(string remotePath, string localPath);
    Task<FtpOperationResult> DeleteFileAsync(string remotePath);
    Task<IEnumerable<string>> ListFilesAsync(string remoteDirectory);
}