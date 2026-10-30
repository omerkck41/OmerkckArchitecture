# Documents

An abstraction for office and media file operations. It supports Excel read/write,
CSV export, and image processing.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Documents.Abstractions` | `IExcelService`, `ICsvExporter`, `IImageProcessor` |
| `Kck.Documents.ClosedXml` | ClosedXML (OpenXML-based `.xlsx`) |
| `Kck.Documents.ImageSharp` | SixLabors.ImageSharp (native-free image processing) |

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

## ImageSharp

```csharp
services.AddKckDocumentsImageSharp();
```

Usage:

```csharp
using Kck.Documents.Abstractions;

public class AvatarService(IImageProcessor images)
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

## Format Support

`IImageProcessor.ConvertFormatAsync(stream, targetFormat)`:

| Format | Encoder |
|---|---|
| `png` | PngEncoder |
| `jpeg` / `jpg` | JpegEncoder (Quality) |
| `webp` | WebpEncoder (Quality) |
| `gif` | GifEncoder |
| `bmp` | BmpEncoder |
| empty / `null` | the source image's format; JPEG if the source is not in this list (e.g. TIFF) |
| any other name (e.g. `avif`) | `NotSupportedException` (before 4.0 it silently produced JPEG) |

## Notes

- ImageSharp **does not require native dependencies**. Unlike SkiaSharp / System.Drawing,
  cross-platform builds are easy.
- ClosedXml supports only `.xlsx`. For `.xls`, consider NPOI (not used in this project).
- Streaming read/write for large Excel files is a future plan.
