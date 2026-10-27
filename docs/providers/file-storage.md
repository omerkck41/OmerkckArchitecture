# File Storage

Dosya saklama ve transfer icin abstraction: FTP, Azure Blob Storage ve Amazon S3 (ve S3 uyumlu servisler) provider'lari.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.FileStorage.Abstractions` | `IFileStorageService`, `IFtpService`, `IFileConverter`, `IFileMetadataReader` |
| `Kck.FileStorage.FluentFtp` | FluentFTP (FTP/FTPS) |
| `Kck.FileStorage.AzureBlob` | Azure Blob Storage (`AddKckFileStorageAzureBlob`) |
| `Kck.FileStorage.AwsS3` | Amazon S3 ve MinIO / Cloudflare R2 / Wasabi (`AddKckFileStorageAwsS3`; `ServiceUrl`, `ForcePathStyle`) |

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

FTP connection'lar pool'landi (bounded channel, `SmtpConnectionPool` paterni).
`PoolSize` tipik olarak 3-10 arasi — yuksek paralel upload icin artir.

Disposed client otomatik discard edilir, disconnected client yeniden acilir.

## Kullanim

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

## Path Normalizasyonu

`PathHelper.Combine("upload/", "/file.txt")` → `upload/file.txt` — trailing /
ve leading / ile ugrasmaz.

## Guvenlik

- **SFTP / FTPS zorunlu** production'da — duz FTP man-in-the-middle'a acik
- Credential'lar Key Vault / Secrets Manager'da saklanmali, appsettings'de degil
- Path traversal korumasi: kullanici input'u dogrudan FTP path'ine yazma —
  `Path.GetFileName()` ile sanitize et
