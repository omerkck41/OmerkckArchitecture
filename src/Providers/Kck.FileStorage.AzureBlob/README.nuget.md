# Kck.FileStorage.AzureBlob

Azure Blob Storage-backed `IFileStorageService` for uploading, downloading, and managing blobs in Azure containers.

## Installation

```bash
dotnet add package Kck.FileStorage.AzureBlob
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckFileStorageAzureBlob(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION")!;
    options.ContainerName = "files";
});

// Upload a file
public class UploadController(IFileStorageService storage)
{
    [HttpPost("upload")]
    public async Task<string> Upload(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return await storage.UploadAsync(stream, file.FileName, file.ContentType, ct);
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | Azure Storage account connection string | required (or `AccountName`) |
| `AccountName` | Storage account name (Managed Identity) | `null` |
| `ContainerName` | Blob container name | required |
| `CreateContainerIfNotExists` | Auto-create container on startup | `true` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
