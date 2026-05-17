# Kck.FileStorage.AwsS3

AWS S3-backed `IFileStorageService` for uploading, downloading, and managing files in Amazon S3 buckets.

## Installation

```bash
dotnet add package Kck.FileStorage.AwsS3
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckFileStorageAwsS3(options =>
{
    options.Region = "eu-central-1";
    options.BucketName = "my-bucket";
    options.AccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY")!;
    options.SecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY")!;
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
| `Region` | AWS region code | required |
| `BucketName` | Target S3 bucket name | required |
| `AccessKey` | AWS access key ID | required (or IAM role) |
| `SecretKey` | AWS secret access key | required (or IAM role) |
| `PublicReadAccess` | Make uploaded objects publicly readable | `false` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
