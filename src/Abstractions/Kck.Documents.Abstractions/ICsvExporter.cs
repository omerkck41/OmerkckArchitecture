namespace Kck.Documents.Abstractions;

/// <summary>
/// Exports data as CSV.
/// </summary>
public interface ICsvExporter
{
    Task<DocumentResult> ExportAsync<T>(IEnumerable<T> data, string fileName = "export.csv", CancellationToken ct = default);
}
