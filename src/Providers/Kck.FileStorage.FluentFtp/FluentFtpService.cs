using FluentFTP;
using Kck.FileStorage.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.FileStorage.FluentFtp;

public sealed class FluentFtpService(
    FtpConnectionPool pool,
    ILogger<FluentFtpService> logger) : IFtpService
{
    public async Task UploadAsync(FileUploadRequest request, CancellationToken ct = default)
    {
        PathSanitizer.Validate(request.DestinationPath);
        PathSanitizer.Validate(request.FileName);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            var path = PathHelper.Combine(request.DestinationPath, request.FileName);
            var status = await client.UploadStream(request.Content, path,
                request.Overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                token: ct).ConfigureAwait(false);

            if (status == FtpStatus.Failed)
                logger.LogWarning("Failed to upload file to {Path}", path);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
    {
        PathSanitizer.Validate(path);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            var ms = new MemoryStream();
            await client.DownloadStream(ms, path, token: ct).ConfigureAwait(false);
            ms.Position = 0;
            return ms;
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        PathSanitizer.Validate(path);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            await client.DeleteFile(path, ct).ConfigureAwait(false);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        PathSanitizer.Validate(path);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            return await client.FileExists(path, ct).ConfigureAwait(false);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyList<FileMetadata>> ListAsync(string directoryPath, CancellationToken ct = default)
    {
        PathSanitizer.Validate(directoryPath);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            var items = await client.GetListing(directoryPath, ct).ConfigureAwait(false);

            return items
                .Where(i => i.Type == FtpObjectType.File)
                .Select(i => new FileMetadata
                {
                    FileName = i.Name,
                    FullPath = i.FullName,
                    SizeInBytes = i.Size,
                    ModifiedAt = i.Modified
                })
                .ToList();
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    public async Task CreateDirectoryAsync(string path, CancellationToken ct = default)
    {
        PathSanitizer.Validate(path);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            await client.CreateDirectory(path, ct).ConfigureAwait(false);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    public async Task DeleteDirectoryAsync(string path, CancellationToken ct = default)
    {
        PathSanitizer.Validate(path);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            await client.DeleteDirectory(path, ct).ConfigureAwait(false);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    public async Task MoveAsync(string sourcePath, string destinationPath, CancellationToken ct = default)
    {
        PathSanitizer.Validate(sourcePath);
        PathSanitizer.Validate(destinationPath);

        var client = await pool.RentAsync(ct).ConfigureAwait(false);
        try
        {
            await client.MoveFile(sourcePath, destinationPath, FtpRemoteExists.Overwrite, ct).ConfigureAwait(false);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }
}
