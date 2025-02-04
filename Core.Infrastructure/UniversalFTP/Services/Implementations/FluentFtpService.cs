using Core.Infrastructure.UniversalFTP.Factories;
using Core.Infrastructure.UniversalFTP.Services.Interfaces;
using Core.Infrastructure.UniversalFTP.Services.Models;
using FluentFTP.Helpers;

namespace Core.Infrastructure.UniversalFTP.Services.Implementations;

public class FluentFtpService : IFtpService
{
    private readonly FtpConnectionPool _connectionPool;

    public FluentFtpService(FtpConnectionPool connectionPool)
    {
        _connectionPool = connectionPool ?? throw new ArgumentNullException(nameof(connectionPool));
    }

    public async Task<FtpOperationResult> UploadFileAsync(string localPath, string remotePath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            var isSuccess = await Task.Run(() => client.UploadFile(localPath, remotePath));
            return isSuccess.IsSuccess()
                ? FtpOperationResult.SuccessResult("File uploaded successfully.")
                : FtpOperationResult.FailureResult("Failed to upload file.");
        }
        catch (Exception ex)
        {
            return FtpOperationResult.FailureResult("Error during file upload.", ex);
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }

    // SplitFileIntoChunks Metodu
    private List<string> SplitFileIntoChunks(string filePath)
    {
        const int chunkSize = 10 * 1024 * 1024; // 10 MB
        var chunkPaths = new List<string>();
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[chunkSize];
        int bytesRead;
        int chunkIndex = 0;

        while ((bytesRead = fileStream.Read(buffer, 0, chunkSize)) > 0)
        {
            var chunkPath = Path.Combine(Path.GetTempPath(), $"chunk_{chunkIndex++}.tmp");
            File.WriteAllBytes(chunkPath, buffer.Take(bytesRead).ToArray());
            chunkPaths.Add(chunkPath);
        }

        return chunkPaths;
    }
    public async Task<FtpOperationResult> UploadLargeFileAsync(string localPath, string remotePath)
    {
        var chunkPaths = SplitFileIntoChunks(localPath);

        await Parallel.ForEachAsync(chunkPaths, async (chunkPath, cancellationToken) =>
        {
            await UploadFileAsync(chunkPath, $"{remotePath}/{Path.GetFileName(chunkPath)}");
        });

        return FtpOperationResult.SuccessResult("Large file uploaded successfully.");
    }

    public async Task<FtpOperationResult> DownloadFileAsync(string remotePath, string localPath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            var isSuccess = await Task.Run(() => client.DownloadFile(localPath, remotePath));
            return isSuccess.IsSuccess()
                ? FtpOperationResult.SuccessResult("File downloaded successfully.")
                : FtpOperationResult.FailureResult("Failed to download file.");
        }
        catch (Exception ex)
        {
            return FtpOperationResult.FailureResult("Error during file download.", ex);
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }

    public async Task<FtpOperationResult> DeleteFileAsync(string remotePath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            await Task.Run(() => client.DeleteFile(remotePath));
            return FtpOperationResult.SuccessResult("File deleted successfully.");
        }
        catch (Exception ex)
        {
            return FtpOperationResult.FailureResult("Error during file deletion.", ex);
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string remoteDirectory)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            return await Task.Run(() => client.GetNameListing(remoteDirectory));
        }
        catch (Exception)
        {
            return Enumerable.Empty<string>();
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }
}