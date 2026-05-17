using FluentFTP;
using Kck.FileStorage.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kck.FileStorage.FluentFtp;

/// <summary>
/// Implementation of <see cref="IFtpService"/> that uses FluentFTP and a connection pool to
/// upload, download, delete, list and manage files on an FTP server.
/// </summary>
public sealed partial class FluentFtpService(
    FtpConnectionPool pool,
    ILogger<FluentFtpService> logger) : IFtpService
{
    /// <summary>
    /// Uploads a file stream to the FTP server at the path specified in
    /// <paramref name="request"/>.  Validates both the destination path and file name before
    /// renting a connection from the pool.
    /// </summary>
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
                LogUploadFailed(logger, path);
        }
        finally
        {
            await pool.ReturnAsync(client).ConfigureAwait(false);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to upload file to {Path}")]
    private static partial void LogUploadFailed(ILogger logger, string path);

    /// <summary>
    /// Downloads the file at <paramref name="path"/> from the FTP server and returns its
    /// contents as a seekable <see cref="MemoryStream"/>.
    /// </summary>
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

    /// <summary>
    /// Deletes the file at <paramref name="path"/> on the FTP server.
    /// </summary>
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

    /// <summary>
    /// Returns <see langword="true"/> when a file exists at <paramref name="path"/> on the FTP
    /// server; otherwise <see langword="false"/>.
    /// </summary>
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

    /// <summary>
    /// Lists all files in <paramref name="directoryPath"/> on the FTP server and returns their
    /// metadata as a read-only collection of <see cref="FileMetadata"/>.
    /// </summary>
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

    /// <summary>
    /// Creates the directory at <paramref name="path"/> on the FTP server, including any missing
    /// intermediate directories.
    /// </summary>
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

    /// <summary>
    /// Recursively deletes the directory at <paramref name="path"/> and all of its contents from
    /// the FTP server.
    /// </summary>
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

    /// <summary>
    /// Moves (renames) a file from <paramref name="sourcePath"/> to
    /// <paramref name="destinationPath"/> on the FTP server, overwriting if necessary.
    /// </summary>
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
