# Documents

Ofis / medya dosya islemleri icin abstraction. Excel okuma-yazma, CSV export
ve gorsel isleme destegi.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Documents.Abstractions` | `IExcelService`, `ICsvExporter`, `IImageProcessor` |
| `Kck.Documents.ClosedXml` | ClosedXML (OpenXML-based `.xlsx`) |
| `Kck.Documents.ImageSharp` | SixLabors.ImageSharp (native-free resim isleme) |

## ClosedXml (Excel)

```csharp
services.AddKckDocumentsClosedXml();
```

Kullanim:

```csharp
public class ReportService(IExcelService excel)
{
    public async Task<byte[]> ExportSalesAsync(IEnumerable<Sale> sales)
    {
        var worksheet = new ExcelWorksheet
        {
            Name = "Sales 2026",
            Headers = ["Id", "Amount", "Date"],
            Rows = sales.Select(s => new object?[] { s.Id, s.Amount, s.Date })
        };

        return await excel.WriteAsync([worksheet]);
    }
}
```

Okuma:

```csharp
await using var stream = File.OpenRead("input.xlsx");
var rows = await excel.ReadAsync(stream);
```

## ImageSharp

```csharp
services.AddKckDocumentsImageSharp();
```

Kullanim:

```csharp
public class AvatarService(IImageProcessor images)
{
    public async Task<byte[]> MakeThumbnailAsync(Stream input)
    {
        return await images.ResizeAsync(input, new ImageProcessingOptions
        {
            Width = 128,
            Height = 128,
            MaintainAspectRatio = true,
            OutputFormat = "webp",
            Quality = 85
        });
    }
}
```

## Format Destegi

`IImageProcessor.ConvertFormatAsync(stream, targetFormat)`:

| Format | Encoder |
|---|---|
| `png` | PngEncoder |
| `jpeg` / `jpg` / bilinmeyen | JpegEncoder (Quality) |
| `webp` | WebpEncoder (Quality) |
| `gif` | GifEncoder |
| `bmp` | BmpEncoder |

## Notlar

- ImageSharp **native dependency gerektirmez** — SkiaSharp / System.Drawing
  tersine cross-platform build kolaydir
- ClosedXml sadece `.xlsx` destekler — `.xls` icin NPOI dusun (bu projede yok)
- Buyuk Excel dosyalari icin streaming read/write gelecekteki plan
