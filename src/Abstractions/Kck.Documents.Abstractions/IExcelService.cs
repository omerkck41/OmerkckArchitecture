namespace Kck.Documents.Abstractions;

/// <summary>
/// Creates and reads Excel workbooks.
/// </summary>
public interface IExcelService
{
    /// <summary>Creates an Excel file from worksheets.</summary>
    Task<DocumentResult> CreateAsync(IEnumerable<ExcelWorksheet> worksheets, CancellationToken ct = default);

    /// <summary>Creates an Excel file from a typed collection (one worksheet, auto-headers).</summary>
    Task<DocumentResult> CreateFromDataAsync<T>(IEnumerable<T> data, string worksheetName = "Sheet1", CancellationToken ct = default);

    /// <summary>Reads all rows from the first worksheet.</summary>
    Task<IReadOnlyList<IReadOnlyList<object?>>> ReadAsync(Stream stream, CancellationToken ct = default);
}
