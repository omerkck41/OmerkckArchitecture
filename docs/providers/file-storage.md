# File Storage

Abstraction for file storage and transfer, with FTP, Azure Blob Storage and Amazon S3 (and S3-compatible services) providers.

## Packages

| Package | Target |
|---|---|
| `Kck.FileStorage.Abstractions` | `IFileStorageService`, `IFtpService`, `IFileConverter`, `IFileMetadataReader` |
| `Kck.FileStorage.FluentFtp` | FluentFTP (FTP/FTPS) |
| `Kck.FileStorage.AzureBlob` | Azure Blob Storage (`AddKckFileStorageAzureBlob`) |
| `Kck.FileStorage.AwsS3` | Amazon S3 and MinIO / Cloudflare R2 / Wasabi (`AddKckFileStorageAwsS3`; `ServiceUrl`, `ForcePathStyle`) |

## FluentFtp Setup

```csharp
services.AddKckFileStorageFluentFtp(opt =>
{
    opt.Host = builder.Configuration["Ftp:Host"]!;
    opt.Username = builder.Configuration["Ftp:User"];
    opt.Password = builder.Configuration["Ftp:Password"];
    opt.Port = 21;
    opt.UseSsl = true;
    opt.PoolSize = 5;
});
```

## Connection Pool

FTP connections are pooled (bounded channel, same pattern as `SmtpConnectionPool`).
`PoolSize` is typically between 3 and 10. Increase it for highly parallel uploads.

A disposed client is discarded automatically, and a disconnected client is reopened.

The options are validated at startup (`ValidateOnStart`). The registration adds `IFtpService` and, for code that
only needs storage operations, `IFileStorageService`, both as singletons (the same instance) over the pool.

## Usage

```csharp
using Kck.FileStorage.Abstractions;

public class ReportUploader(IFileStorageService storage)
{
    public Task UploadAsync(Stream content, string fileName, CancellationToken ct) =>
        storage.UploadAsync(new FileUploadRequest
        {
            Content = content,
            FileName = Path.GetFileName(fileName), // never trust a client-supplied path
            DestinationPath = "reports",
            Overwrite = true
        }, ct);

    public Task<Stream> DownloadAsync(string path, CancellationToken ct) => storage.DownloadAsync(path, ct);

    public Task<bool> ExistsAsync(string path, CancellationToken ct) => storage.ExistsAsync(path, ct);

    public Task<IReadOnlyList<FileMetadata>> ListAsync(string directory, CancellationToken ct) =>
        storage.ListAsync(directory, ct);

    public Task DeleteAsync(string path, CancellationToken ct) => storage.DeleteAsync(path, ct);
}
```

`IFtpService` adds `CreateDirectoryAsync`, `DeleteDirectoryAsync` and `MoveAsync`.

With the FTP provider, `UploadAsync` throws `IOException` when the server reports a failed upload, or when
`Overwrite` is `false` and the file already exists. Before 5.0 both returned silently (a failure only logged a
warning).

With the Azure Blob provider and `CreateContainerIfNotExists = true` (the default), the container is created on first
use, not when the service is constructed; a failed attempt is retried on the next call.

## Path Helpers

`PathHelper.Combine("upload", "2026", "file.txt")` → `upload/2026/file.txt` (backslashes become forward slashes).
`PathHelper.IsValidPath(path)` rejects `..` and `~` segments and invalid characters; `PathHelper.Sanitize(path)` removes
them.

`PathHelper.Validate(path)` throws `ArgumentException` for a null or white-space path, a path that contains `..`
anywhere (also `report..v2.pdf`), an absolute path (leading `/` or `\`), a percent sign (URL-encoded traversal such as
`%2e%2e`) or a control character. The FTP, Azure Blob and S3 providers call it on every path argument (`path`,
`directoryPath`, `FileName`, `DestinationPath`) before a request is built, so such a path never reaches the service;
for Blob and S3 an empty `DestinationPath` or `directoryPath` means the container or bucket root (under `KeyPrefix` for
S3). In Azure Blob the blob name is part of the request URI, where `..` would otherwise resolve
to another container of the same account (before 5.0.0, `DownloadAsync("../beta/secret.txt")` on a service bound to
container `alpha` returned `beta`'s blob).

## Security

- **Use FTPS in production** (`UseSsl = true`). Plain FTP is open to man-in-the-middle attacks; FluentFTP does not
  support SFTP.
- Store credentials in Key Vault / Secrets Manager, not in appsettings.
- Path traversal protection: never write user input directly into a storage path. Take the file name with
  `Path.GetFileName()` and check paths with `PathHelper.IsValidPath()`. The providers themselves reject `..`, absolute,
  percent-encoded and control-character paths with `ArgumentException` (`PathHelper.Validate`), so a request with such
  a path fails before it reaches the service.
