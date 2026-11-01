# Kck.Documents.SkiaSharp

SkiaSharp-backed `IImageProcessor` for server-side image resizing, format conversion and dimension reading.
SkiaSharp is MIT-licensed. This package replaces `Kck.Documents.ImageSharp`, which is removed in 5.0.

> **Linux:** add `SkiaSharp.NativeAssets.Linux.NoDependencies` (glibc and musl/Alpine) to the application; on
> Windows and macOS the SkiaSharp package carries the native library.
>
> ```bash
> dotnet add package SkiaSharp.NativeAssets.Linux.NoDependencies
> ```

## Installation

```bash
dotnet add package Kck.Documents.SkiaSharp
```

## Quick Start

```csharp
using Kck.Documents.Abstractions;

// Program.cs
builder.Services.AddKckDocumentsSkiaSharp();

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

## Formats

| | Formats |
|---|---|
| Input (decode) | JPEG, PNG, WebP, GIF, BMP, ICO |
| Output (encode) | `jpeg` (`jpg`), `png`, `webp` |

An empty `OutputFormat` / `ConvertFormatAsync` target keeps the source format; a GIF, BMP or ICO source becomes JPEG.
Any other name, including `gif` and `bmp`, throws `NotSupportedException`. A stream SkiaSharp cannot decode (for
example TIFF) and truncated or corrupt image data throw `InvalidDataException`.

## Options

`ImageProcessingOptions` (per call):

| Property | Description | Default |
|---|---|---|
| `Width` / `Height` | Target size in pixels. One side set (the other `null` or `0`): the other follows the source ratio; neither set: the size is kept; negative throws `ArgumentOutOfRangeException` before the stream is read | `null` |
| `MaintainAspectRatio` | With both sides set: fit inside the box (scaling up or down) instead of stretching | `true` |
| `OutputFormat` | `jpeg` (`jpg`), `png`, `webp`; `null` keeps the source format | `null` |
| `Quality` | Encoder quality, clamped to 1-100 (JPEG, WebP; WebP 100 is lossless); PNG ignores it | `80` |

`SkiaSharpOptions` (registration):

| Property | Description | Default |
|---|---|---|
| `MaxPixels` | Largest accepted image (width × height), for the source and for the resize target. Both are checked from the header before any pixel is decoded, so a decoder bomb or an oversized request is rejected with `InvalidOperationException`. Must be at least 1 | `50_000_000` |

```csharp
builder.Services.AddKckDocumentsSkiaSharp(options => options.MaxPixels = 20_000_000);
```

## Notes

- `GetDimensionsAsync` reads only the image header (no pixels are decoded) and returns the displayed size
  (orientation applied, like the `ResizeAsync` output); `MaxPixels` does not apply to it, so it can check an image
  first.
- Converting an image with transparency to JPEG fills the transparent areas with white.
- EXIF orientation is applied: a photo stored rotated comes out upright (the rotation is baked into the pixels).
- The output carries no metadata: EXIF (including GPS location) and the ICC profile are not copied. Pixels are
  converted to sRGB, so Display P3 or Adobe RGB sources keep their colours without an embedded profile.
- A non-seekable stream (for example a request body) is fully buffered in memory first; limit the request body size
  (Kestrel `MaxRequestBodySize`, `FormOptions.MultipartBodyLengthLimit`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md)
- [Migration from ImageSharp](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/migrations/v4-to-v5.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
