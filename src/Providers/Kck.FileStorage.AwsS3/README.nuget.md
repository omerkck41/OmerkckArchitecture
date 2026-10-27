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
    public async Task Upload(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        await storage.UploadAsync(new FileUploadRequest
        {
            Content = stream,
            FileName = file.FileName,
            DestinationPath = "uploads"
        }, ct);
    }
}
```

## S3-compatible services (MinIO, Cloudflare R2, Wasabi)

```csharp
builder.Services.AddKckFileStorageAwsS3(options =>
{
    options.Region = "us-east-1";                 // used to sign requests
    options.BucketName = "my-bucket";
    options.ServiceUrl = "http://localhost:9000"; // the service endpoint
    options.ForcePathStyle = true;                // most self-hosted services need path-style URLs
    options.ChecksumWhenRequired = true;          // if the service rejects AWS SDK v4 default checksums
    options.AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY")!;
    options.SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY")!;
});
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Region` | AWS region code (with `ServiceUrl`: signing region only) | required |
| `BucketName` | Target S3 bucket name | required |
| `AccessKey` | AWS access key ID | `null` (IAM role / environment) |
| `SecretKey` | AWS secret access key; set together with `AccessKey` | `null` (IAM role / environment) |
| `KeyPrefix` | Prefix prepended to every object key (tenant isolation) | `null` |
| `ServiceUrl` | Absolute http(s) endpoint of an S3-compatible service | `null` (Amazon S3) |
| `ForcePathStyle` | Path-style URLs (`host/bucket/key`) instead of virtual-hosted | `false` |
| `ChecksumWhenRequired` | Compute/validate checksums only when the operation requires them | `false` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
