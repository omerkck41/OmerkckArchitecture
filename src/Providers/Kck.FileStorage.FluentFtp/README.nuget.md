# Kck.FileStorage.FluentFtp

FluentFTP-backed `IFileStorageService` for transferring files over FTP/FTPS with connection pooling support.

## Installation

```bash
dotnet add package Kck.FileStorage.FluentFtp
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckFileStorageFluentFtp(options =>
{
    options.Host = "ftp.example.com";
    options.Port = 21;
    options.Username = Environment.GetEnvironmentVariable("FTP_USER")!;
    options.Password = Environment.GetEnvironmentVariable("FTP_PASSWORD")!;
    options.UseSsl = true;
});

// Upload a file
public class BackupService(IFileStorageService storage)
{
    public async Task BackupAsync(Stream data, string path, CancellationToken ct)
        => await storage.UploadAsync(data, path, "application/octet-stream", ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Host` | FTP server hostname | required |
| `Port` | FTP server port | `21` |
| `Username` | FTP account username | required |
| `Password` | FTP account password | required |
| `UseSsl` | Enable FTPS (implicit TLS) | `false` |
| `PoolSize` | FTP connection pool size | `4` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
