using Core.Application.UniversalFTP.Services.Models;

namespace Core.Application.UniversalFTP.Services.Interfaces;

public interface IChunkedFtpService
{
    Task<FtpOperationResult> UploadFileInChunksAsync(string localPath, string remotePath, int chunkSizeInBytes);
}