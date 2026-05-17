# Kck.Documents.ClosedXml

ClosedXML-backed `IExcelService` for generating and reading Excel workbooks from strongly-typed .NET collections.

## Installation

```bash
dotnet add package Kck.Documents.ClosedXml
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckDocumentsClosedXml();

// Generate Excel from a list
public class ReportController(IExcelService excel)
{
    [HttpGet("export")]
    public async Task<FileResult> Export(CancellationToken ct)
    {
        var orders = await _db.Orders.ToListAsync(ct);
        var stream = await excel.CreateFromDataAsync(orders, ct);
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "orders.xlsx");
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `DefaultSheetName` | Default worksheet name | `"Sheet1"` |
| `MaxRowsPerSheet` | Row limit before creating a new sheet | `1048576` |
| `DateFormat` | Cell format string for DateTime columns | `"yyyy-MM-dd"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
