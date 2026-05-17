# Kck.Documents.ImageSharp

SixLabors.ImageSharp-backed `IImageProcessor` for server-side image resizing, format conversion, and quality adjustment.

## Installation

```bash
dotnet add package Kck.Documents.ImageSharp
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckDocumentsImageSharp();

// Resize and convert
public class ImageController(IImageProcessor images)
{
    [HttpPost("thumbnail")]
    public async Task<IActionResult> Thumbnail(IFormFile file, CancellationToken ct)
    {
        await using var input = file.OpenReadStream();
        var result = await images.ResizeAsync(input, width: 200, height: 200, ct);
        return File(result, "image/webp");
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `DefaultOutputFormat` | Output image format (`webp`, `jpeg`, `png`) | `"webp"` |
| `DefaultQuality` | Output quality 1-100 | `85` |
| `MaxImageWidth` | Maximum allowed input width in pixels | `8000` |
| `MaxImageHeight` | Maximum allowed input height in pixels | `8000` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
