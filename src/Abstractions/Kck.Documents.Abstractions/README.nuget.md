# Kck.Documents.Abstractions

Provider-agnostic document processing contracts for Excel generation, CSV export, and image resizing — implement with ClosedXML or ImageSharp providers.

## Installation

```bash
dotnet add package Kck.Documents.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider
builder.Services.AddKckClosedXmlDocuments();
builder.Services.AddKckImageSharpProcessor();

// Generate an Excel file from a list
public class ReportHandler(IExcelService excel, ICsvExporter csv)
{
    public async Task<byte[]> ExportOrdersExcelAsync(
        IReadOnlyList<OrderRow> rows, CancellationToken ct)
    {
        return await excel.CreateFromDataAsync(rows, sheetName: "Orders", ct);
    }

    public async Task<byte[]> ExportOrdersCsvAsync(
        IReadOnlyList<OrderRow> rows, CancellationToken ct)
    {
        return await csv.ExportAsync(rows, ct);
    }
}

// Resize an uploaded image
public async Task<byte[]> ThumbnailAsync(
    Stream imageStream, IImageProcessor processor, CancellationToken ct)
{
    return await processor.ResizeAsync(imageStream, width: 200, height: 200, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Documents:Excel:DefaultSheetName` | Default worksheet name | `"Sheet1"` |
| `Documents:Image:JpegQuality` | JPEG compression quality (1-100) | `85` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
