# Kck.FileStorage.FluentFtp

FluentFTP-backed `IFileStorageService` for transferring files over FTP/FTPS with connection pooling support.

## Installation

```bash
dotnet add package Kck.FileStorage.FluentFtp
```

## Quick Start

```csharp
using Kck.FileStorage.Abstractions;

// Program.cs
builder.Services.AddKckFileStorageFluentFtp(options =>
{
    options.Host = "ftp.example.com";
    options.Port = 21;
    options.Username = Environment.GetEnvironmentVariable("FTP_USER");
    options.Password = Environment.GetEnvironmentVariable("FTP_PASSWORD");
    options.UseSsl = true; // FTPS; plain FTP sends credentials in clear text
});

// Upload a file (IFtpService adds directory and move operations)
public class BackupService(IFileStorageService storage)
{
    public Task BackupAsync(Stream data, string fileName, CancellationToken ct) =>
        storage.UploadAsync(new FileUploadRequest
        {
            Content = data,
            FileName = fileName,
            DestinationPath = "backups",
            Overwrite = true
        }, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Host` | FTP server | required |
| `Port` | FTP port | `21` |
| `Username` | FTP user | `null` |
| `Password` | FTP password | `null` |
| `UseSsl` | FTPS (use it in production; plain FTP sends credentials in clear text) | `false` |
| `PoolSize` | FTP connections kept in the pool | `5` |

The options are validated at startup. `IFtpService` and `IFileStorageService` are registered as singletons over the
connection pool.

## Upload

`UploadAsync` throws `IOException` when the server reports a failed upload, or when `Overwrite` is `false` and the
file already exists. Before 5.0 both returned silently (a failure only logged a warning).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
