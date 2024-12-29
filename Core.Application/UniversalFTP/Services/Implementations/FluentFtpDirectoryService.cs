using Core.Application.UniversalFTP.Factories;
using Core.Application.UniversalFTP.Services.Interfaces;
using Core.Application.UniversalFTP.Services.Models;

namespace Core.Application.UniversalFTP.Services.Implementations;

public class FluentFtpDirectoryService : IFtpDirectoryService
{
    private readonly FtpConnectionPool _connectionPool;

    public FluentFtpDirectoryService(FtpConnectionPool connectionPool)
    {
        _connectionPool = connectionPool ?? throw new ArgumentNullException(nameof(connectionPool));
    }


    public async Task<FtpOperationResult> CreateDirectoryAsync(string remotePath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            bool success = await Task.Run(() => client.CreateDirectory(remotePath));
            return success
                ? FtpOperationResult.SuccessResult($"Directory '{remotePath}' created successfully.")
                : FtpOperationResult.FailureResult($"Failed to create directory '{remotePath}'.");
        }
        catch (Exception ex)
        {
            return FtpOperationResult.FailureResult($"Error creating directory '{remotePath}'.", ex);
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }

    public async Task<FtpOperationResult> DeleteDirectoryAsync(string remotePath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            await Task.Run(() => client.DeleteDirectory(remotePath));
            return FtpOperationResult.SuccessResult($"Directory '{remotePath}' deleted successfully.");
        }
        catch (Exception ex)
        {
            return FtpOperationResult.FailureResult($"Error deleting directory '{remotePath}'.", ex);
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }

    public async Task<FtpOperationResult> MoveDirectoryAsync(string oldPath, string newPath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            await Task.Run(() => client.MoveDirectory(oldPath, newPath));
            return FtpOperationResult.SuccessResult($"Directory moved from '{oldPath}' to '{newPath}' successfully.");
        }
        catch (Exception ex)
        {
            return FtpOperationResult.FailureResult($"Error moving directory from '{oldPath}' to '{newPath}'.", ex);
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string remotePath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            return await Task.Run(() => client.GetNameListing(remotePath));
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }

    public async Task<bool> DirectoryExistsAsync(string remotePath)
    {
        var client = await _connectionPool.GetClientAsync();
        try
        {
            return await Task.Run(() => client.DirectoryExists(remotePath));
        }
        finally
        {
            _connectionPool.ReleaseClient(client);
        }
    }
}