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
    opt.UserName = builder.Configuration["Ftp:User"]!;
    opt.Password = builder.Configuration["Ftp:Password"]!;
    opt.Port = 21;
    opt.UseSsl = true;
    opt.PoolSize = 5;
});
```

## Connection Pool

FTP connections are pooled (bounded channel, same pattern as `SmtpConnectionPool`).
`PoolSize` is typically between 3 and 10. Increase it for highly parallel uploads.

A disposed client is discarded automatically, and a disconnected client is reopened.

## Usage

```csharp
public class ReportUploader(IFtpService ftp)
{
    public async Task UploadAsync(Stream content, string remotePath, CancellationToken ct)
    {
        await ftp.UploadAsync(content, remotePath, ct);
    }

    public async Task<Stream> DownloadAsync(string remotePath, CancellationToken ct)
    {
        return await ftp.DownloadAsync(remotePath, ct);
    }

    public async Task DeleteAsync(string remotePath, CancellationToken ct)
    {
        await ftp.DeleteAsync(remotePath, ct);
    }

    public async Task<bool> ExistsAsync(string remotePath, CancellationToken ct)
    {
        return await ftp.ExistsAsync(remotePath, ct);
    }
}
```

## Path Normalization

`PathHelper.Combine("upload/", "/file.txt")` → `upload/file.txt`. You do not
need to handle trailing or leading slashes yourself.

## Security

- **Use SFTP / FTPS in production.** Plain FTP is open to man-in-the-middle attacks.
- Store credentials in Key Vault / Secrets Manager, not in appsettings.
- Path traversal protection: never write user input directly into an FTP path.
  Sanitize it with `Path.GetFileName()`.
