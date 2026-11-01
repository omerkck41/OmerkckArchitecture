# Documents

An abstraction for office and media file operations. It supports Excel read/write,
CSV export, and image processing.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Documents.Abstractions` | `IExcelService`, `ICsvExporter`, `IImageProcessor` |
| `Kck.Documents.ClosedXml` | ClosedXML (OpenXML-based `.xlsx`) |
| `Kck.Documents.SkiaSharp` | SkiaSharp (MIT) image processing |

## ClosedXml (Excel)

```csharp
services.AddKckDocumentsClosedXml();
```

Usage:

```csharp
using Kck.Documents.Abstractions;

public class ReportService(IExcelService excel)
{
    public async Task<byte[]> ExportSalesAsync(IEnumerable<Sale> sales, CancellationToken ct)
    {
        var worksheet = new ExcelWorksheet
        {
            Name = "Sales 2026",
            Headers = ["Id", "Amount", "Date"],
            Rows = sales.Select(s => (IReadOnlyList<object?>)[s.Id, s.Amount, s.Date]).ToList()
        };

        DocumentResult result = await excel.CreateAsync([worksheet], ct);
        return result.Content; // also FileName and ContentType for a download response
    }

    public async Task<int> CountRowsAsync(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        var rows = await excel.ReadAsync(stream, ct); // first worksheet, one list of cell values per row
        return rows.Count;
    }
}
```

For a list of objects, `CreateFromDataAsync(items, "Sheet1")` writes one column per public property; `ICsvExporter`
(also registered by `AddKckDocumentsClosedXml`) does the same as CSV.

## SkiaSharp

```csharp
services.AddKckDocumentsSkiaSharp();

// or with a lower decoder-bomb limit (default 50,000,000 pixels)
services.AddKckDocumentsSkiaSharp(options => options.MaxPixels = 20_000_000);
```

Usage:

```csharp
using Kck.Documents.Abstractions;

public class ThumbnailService(IImageProcessor images)
{
    public Task<byte[]> MakeThumbnailAsync(Stream input, CancellationToken ct) =>
        images.ResizeAsync(input, new ImageProcessingOptions
        {
            Width = 128,
            Height = 128,
            MaintainAspectRatio = true,
            OutputFormat = "webp",
            Quality = 85
        }, ct);
}
```

**Linux:** add `SkiaSharp.NativeAssets.Linux.NoDependencies` (glibc and musl/Alpine) to the application; on Windows
and macOS the `SkiaSharp` package carries the native library. Without it the first call fails because
`libSkiaSharp` cannot be loaded.

| | Formats |
|---|---|
| Input (decode) | JPEG, PNG, WebP, GIF, BMP, ICO — anything else (e.g. TIFF) and truncated or corrupt data throw `InvalidDataException` |
| Output (encode) | `jpeg` / `jpg` (Quality), `png`, `webp` (Quality; 100 is lossless) |
| empty / `null` output | the source format; JPEG when the source is GIF, BMP or ICO |
| any other output name, including `gif` and `bmp` | `NotSupportedException` listing the supported formats |

- `SkiaSharpOptions.MaxPixels` (default 50,000,000, at least 1, validated at start-up) applies to the source and to the
  resize target: an image whose header declares more pixels, or a requested size above the limit, is rejected with
  `InvalidOperationException` before any pixel is decoded.
- `GetDimensionsAsync` reads only the header (no pixels are decoded) and returns the displayed size (orientation
  applied, like the `ResizeAsync` output); `MaxPixels` does not apply to it.
- A non-seekable stream (for example a request body) is fully buffered in memory first; limit the request body size
  (Kestrel `MaxRequestBodySize`, `FormOptions.MultipartBodyLengthLimit`).
- `MaintainAspectRatio = true` fits the image inside the box, scaling up or down (200×100 into 100×100 → 100×50).
- With only `Width` or only `Height` set (the other `null` or `0`) the other side follows the source ratio (200×100
  with `Height = 50` → 100×50), whatever `MaintainAspectRatio` says; with neither set the size is kept. A negative
  value throws `ArgumentOutOfRangeException` before the stream is read. `Quality` is clamped to 1–100.
- An image with transparency converted to JPEG gets a white background.
- EXIF orientation is applied (baked into the pixels). The output carries no metadata: EXIF (including GPS) and the ICC
  profile are not copied; pixels are converted to sRGB.

## Notes

- SkiaSharp uses a native library: on Linux the application must reference
  `SkiaSharp.NativeAssets.Linux.NoDependencies`; Windows and macOS need nothing extra.
- ClosedXml supports only `.xlsx`. For `.xls`, consider NPOI (not used in this project).
- Streaming read/write for large Excel files is a future plan.
