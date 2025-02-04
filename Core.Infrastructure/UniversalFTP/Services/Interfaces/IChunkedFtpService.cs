using Core.Infrastructure.UniversalFTP.Services.Models;

namespace Core.Infrastructure.UniversalFTP.Services.Interfaces;

public interface IChunkedFtpService
{
    Task<FtpOperationResult> UploadFileInChunksAsync(string localPath, string remotePath, int chunkSizeInBytes);
}