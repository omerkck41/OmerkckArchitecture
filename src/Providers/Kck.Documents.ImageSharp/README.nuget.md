# Kck.Documents.ImageSharp

SixLabors.ImageSharp-backed `IImageProcessor` for server-side image resizing, format conversion, and quality adjustment.

> **Dependency:** SixLabors.ImageSharp 4.x (fixes GHSA-j3p4-wp97-rph4, GHSA-j9gm-c75j-xc9q, GHSA-jjfr-hcj7-qf5w,
> GHSA-gwg2-r3hj-4w44 and GHSA-wmxv-xphr-5c9g present in 3.x). ImageSharp is licensed under the
> [Six Labors Split License](https://github.com/SixLabors/ImageSharp/blob/main/LICENSE); if your application also
> references ImageSharp directly, check whether your use requires a Six Labors license.

## Installation

```bash
dotnet add package Kck.Documents.ImageSharp
```

## Quick Start

```csharp
using Kck.Documents.Abstractions;

// Program.cs
builder.Services.AddKckDocumentsImageSharp();

// Resize and convert an uploaded image
app.MapPost("/thumbnail", async (IFormFile file, IImageProcessor images, CancellationToken ct) =>
{
    await using var input = file.OpenReadStream();
    var webp = await images.ResizeAsync(input, new ImageProcessingOptions
    {
        Width = 200,
        Height = 200,
        OutputFormat = "webp",
        Quality = 85
    }, ct);
    return Results.File(webp, "image/webp");
}).DisableAntiforgery();
```

## Options (`ImageProcessingOptions`)

| Property | Description | Default |
|---|---|---|
| `Width` / `Height` | Target size in pixels; `null` keeps the source dimension | `null` |
| `MaintainAspectRatio` | Fit inside the box instead of stretching | `true` |
| `OutputFormat` | `jpeg` (`jpg`), `png`, `webp`, `gif`, `bmp`; `null` keeps the source format (JPEG if the source format is not one of these) | `null` |
| `Quality` | Encoder quality 1-100 (JPEG, WebP) | `80` |

Any other `OutputFormat` / `ConvertFormatAsync` target (for example `avif`) throws `NotSupportedException`.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
