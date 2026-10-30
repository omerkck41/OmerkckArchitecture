# Kck.FileStorage.Abstractions

Provider-agnostic file storage abstractions — Upload, Download, Delete, Exists, and List operations backed by FTP, Azure Blob, or AWS S3 providers.

## Installation

```bash
dotnet add package Kck.FileStorage.Abstractions
```

## Quick Start

```csharp
using Kck.FileStorage.Abstractions;

// Program.cs — a provider (FluentFtp, AzureBlob or AwsS3)
builder.Services.AddKckFileStorageAzureBlob(opt =>
{
    opt.ConnectionString = builder.Configuration["Storage:ConnectionString"];
    opt.ContainerName = "documents";
});

// Use IFileStorageService in a handler
public class DocumentHandler(IFileStorageService storage)
{
    public Task UploadAsync(Stream content, string fileName, CancellationToken ct) =>
        storage.UploadAsync(new FileUploadRequest
        {
            Content = content,
            FileName = Path.GetFileName(fileName), // never trust a client-supplied path
            DestinationPath = "documents"
        }, ct);

    public Task<Stream> DownloadAsync(string path, CancellationToken ct) => storage.DownloadAsync(path, ct);

    public Task DeleteAsync(string path, CancellationToken ct) => storage.DeleteAsync(path, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `FileStorage:Provider` | `FluentFtp`, `AzureBlob`, `AwsS3` | `FluentFtp` |
| `FileStorage:BasePath` | Root path / container name | `"uploads"` |
| `FileStorage:MaxFileSizeBytes` | Upload size guard | `104857600` (100 MB) |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
