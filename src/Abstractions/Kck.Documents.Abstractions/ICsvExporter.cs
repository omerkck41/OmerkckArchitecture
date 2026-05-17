using System.Diagnostics.CodeAnalysis;

namespace Kck.Documents.Abstractions;

/// <summary>
/// Exports data as CSV.
/// </summary>
public interface ICsvExporter
{
    /// <summary>
    /// Exports data collection to CSV format and returns the result as a DocumentResult.
    /// </summary>
    Task<DocumentResult> ExportAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> data, string fileName = "export.csv", CancellationToken ct = default);
}
