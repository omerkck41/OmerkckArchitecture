# Kck.Documents.ClosedXml

ClosedXML-backed `IExcelService` for generating and reading Excel workbooks from strongly-typed .NET collections.

## Installation

```bash
dotnet add package Kck.Documents.ClosedXml
```

## Quick Start

```csharp
using Kck.Documents.Abstractions;

// Program.cs
builder.Services.AddKckDocumentsClosedXml();   // IExcelService + ICsvExporter

public sealed record OrderRow(Guid Id, string Customer, decimal Total);

// Generate an Excel file from a list
app.MapGet("/orders/export", async (IExcelService excel, CancellationToken ct) =>
{
    IReadOnlyList<OrderRow> rows = [new(Guid.NewGuid(), "Ada", 42m)];
    DocumentResult file = await excel.CreateFromDataAsync(rows, worksheetName: "Orders", ct: ct);
    return Results.File(file.Content, file.ContentType, "orders.xlsx");
});
```

## Configuration

No options. `AddKckDocumentsClosedXml()` registers `IExcelService` and `ICsvExporter` (`.xlsx` only; `ReadAsync` reads the first worksheet; every row is as wide as the sheet's last used column and an empty cell is `null`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/documents.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
