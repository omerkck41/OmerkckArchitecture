# Kck.Documents.Abstractions

Provider-agnostic document processing contracts for Excel generation, CSV export, and image resizing — implement with the ClosedXML or SkiaSharp providers.

## Installation

```bash
dotnet add package Kck.Documents.Abstractions
```

## Quick Start

```csharp
using Kck.Documents.Abstractions;

// Program.cs — register concrete providers
builder.Services.AddKckDocumentsClosedXml();   // IExcelService + ICsvExporter
builder.Services.AddKckDocumentsSkiaSharp();  // IImageProcessor

public sealed record OrderRow(Guid Id, string Customer, decimal Total);

// Generate Excel / CSV files from a list
public class ReportHandler(IExcelService excel, ICsvExporter csv)
{
    public async Task<byte[]> ExportOrdersExcelAsync(IReadOnlyList<OrderRow> rows, CancellationToken ct)
    {
        DocumentResult result = await excel.CreateFromDataAsync(rows, worksheetName: "Orders", ct: ct);
        return result.Content; // also: result.ContentType, result.FileName
    }

    public async Task<byte[]> ExportOrdersCsvAsync(IReadOnlyList<OrderRow> rows, CancellationToken ct)
    {
        DocumentResult result = await csv.ExportAsync(rows, fileName: "orders.csv", ct: ct);
        return result.Content;
    }
}

// Resize an uploaded image
public class AvatarService(IImageProcessor processor)
{
    public Task<byte[]> ThumbnailAsync(Stream imageStream, CancellationToken ct) =>
        processor.ResizeAsync(imageStream, new ImageProcessingOptions { Width = 200, Height = 200, OutputFormat = "webp" }, ct);
}
```

## Options

There is no configuration section; behaviour is set per call.

| Setting | Where | Default |
|---|---|---|
| Worksheet name | `CreateFromDataAsync(..., worksheetName)` | `"Sheet1"` |
| CSV file name | `ExportAsync(..., fileName)` | `"export.csv"` |
| Resize size / aspect ratio | `ImageProcessingOptions.Width`, `Height`, `MaintainAspectRatio` | aspect ratio kept |
| Output format | `ImageProcessingOptions.OutputFormat` / `ConvertFormatAsync(..., targetFormat)` — SkiaSharp provider: `jpeg` (`jpg`), `png`, `webp` | `jpeg` |
| Quality (JPEG / WebP) | `ImageProcessingOptions.Quality` | `80` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
