using System.Globalization;
using System.Reflection;
using ClosedXML.Excel;
using Kck.Documents.Abstractions;

namespace Kck.Documents.ClosedXml;

public sealed class ClosedXmlExcelService : IExcelService
{
    public Task<DocumentResult> CreateAsync(IEnumerable<ExcelWorksheet> worksheets, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();

        foreach (var ws in worksheets)
        {
            var sheet = workbook.Worksheets.Add(ws.Name);
            var row = 1;

            if (ws.Headers is { Count: > 0 })
            {
                for (var col = 0; col < ws.Headers.Count; col++)
                    sheet.Cell(row, col + 1).Value = ws.Headers[col];
                row++;
            }

            foreach (var dataRow in ws.Rows)
            {
                for (var col = 0; col < dataRow.Count; col++)
                    sheet.Cell(row, col + 1).SetValue(XLCellValue.FromObject(dataRow[col], CultureInfo.InvariantCulture));
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        return Task.FromResult(ToResult(workbook, "export.xlsx"));
    }

    public Task<DocumentResult> CreateFromDataAsync<T>(IEnumerable<T> data, string worksheetName = "Sheet1", CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(worksheetName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (var col = 0; col < properties.Length; col++)
            sheet.Cell(1, col + 1).Value = properties[col].Name;

        var row = 2;
        foreach (var item in data)
        {
            for (var col = 0; col < properties.Length; col++)
                sheet.Cell(row, col + 1).SetValue(XLCellValue.FromObject(properties[col].GetValue(item), CultureInfo.InvariantCulture));
            row++;
        }

        sheet.Columns().AdjustToContents();
        return Task.FromResult(ToResult(workbook, $"{worksheetName}.xlsx"));
    }

    public Task<IReadOnlyList<IReadOnlyList<object?>>> ReadAsync(Stream stream, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);
        var rows = new List<IReadOnlyList<object?>>();

        foreach (var row in sheet.RowsUsed())
        {
            var cells = new List<object?>();
            foreach (var cell in row.CellsUsed())
                cells.Add(cell.Value.ToString(CultureInfo.InvariantCulture));
            rows.Add(cells);
        }

        return Task.FromResult<IReadOnlyList<IReadOnlyList<object?>>>(rows);
    }

    private static DocumentResult ToResult(XLWorkbook workbook, string fileName)
    {
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return new DocumentResult
        {
            Content = ms.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = fileName
        };
    }
}
