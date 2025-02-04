using Core.Infrastructure.UniversalFTP.Factories;
using Core.Infrastructure.UniversalFTP.Services.Interfaces;
using Core.Infrastructure.UniversalFTP.Services.Models;
using FluentFTP;

namespace Core.Infrastructure.UniversalFTP.Services.Implementations;

public class ChunkedFtpService : IChunkedFtpService
{
    private readonly FtpConnectionPool _connectionPool;

    public ChunkedFtpService(FtpConnectionPool connectionPool)
    {
        _connectionPool = connectionPool;
    }

    public async Task<FtpOperationResult> UploadFileInChunksAsync(string localPath, string remotePath, int chunkSizeInBytes)
    {
        if (!File.Exists(localPath))
            return FtpOperationResult.FailureResult("Local file does not exist.");

        try
        {
            using var fileStream = File.OpenRead(localPath);
            byte[] buffer = new byte[chunkSizeInBytes];
            int bytesRead;
            int chunkIndex = 0;

            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, chunkSizeInBytes)) > 0)
            {
                string chunkPath = $"{remotePath}.part{chunkIndex}";
                await UploadChunkAsync(buffer, bytesRead, chunkPath);
                chunkIndex++;
            }

            return FtpOperationResult.SuccessResult("File uploaded in chunks successfully.");
        }
        catch (Exception ex)
        {
            return FtpOperationResult.FailureResult("Error during chunked upload.", ex);
        }
    }

    private async Task UploadChunkAsync(byte[] buffer, int bytesRead, string chunkPath)
    {
        FtpClient client = await _connectionPool.GetClientAsync();
        try
        {
            using var memoryStream = new MemoryStream(buffer, 0, bytesRead);
            await Task.Run(() => client.UploadStream(memoryStream, chunkPath));
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }
}