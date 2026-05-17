# Kck.FileStorage.Abstractions

Provider-agnostic file storage abstractions — Upload, Download, Delete, Exists, and List operations backed by FTP, Azure Blob, or AWS S3 providers.

## Installation

```bash
dotnet add package Kck.FileStorage.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.FileStorage.FluentFtp)
builder.Services.AddKckFluentFtpStorage(builder.Configuration);

// Use IFileStorageService in a handler
public class DocumentUploadHandler(IFileStorageService storage)
{
    public async Task<string> UploadAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken ct)
    {
        var request = new FileUploadRequest
        {
            FileName  = fileName,
            Content   = fileStream,
            ContentType = contentType,
            Path      = "documents/"
        };

        var metadata = await storage.UploadAsync(request, ct);
        return metadata.Url;
    }

    public async Task<Stream> DownloadAsync(string filePath, CancellationToken ct)
        => await storage.DownloadAsync(filePath, ct);

    public async Task DeleteAsync(string filePath, CancellationToken ct)
        => await storage.DeleteAsync(filePath, ct);
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
