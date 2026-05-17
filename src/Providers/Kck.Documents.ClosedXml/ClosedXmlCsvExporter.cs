using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Kck.Documents.Abstractions;

namespace Kck.Documents.ClosedXml;

/// <summary>
/// Implementation of <see cref="ICsvExporter"/> that uses reflection to enumerate public
/// properties of the exported type and writes RFC-4180-compliant CSV content.
/// </summary>
public sealed class ClosedXmlCsvExporter : ICsvExporter
{
    /// <summary>
    /// Exports <paramref name="data"/> to a UTF-8 CSV <see cref="DocumentResult"/> whose
    /// file name defaults to <c>export.csv</c>.
    /// </summary>
    public Task<DocumentResult> ExportAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> data, string fileName = "export.csv", CancellationToken ct = default)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsv(p.Name))));

        foreach (var item in data)
        {
            var values = properties.Select(p => EscapeCsv(p.GetValue(item)?.ToString() ?? ""));
            sb.AppendLine(string.Join(",", values));
        }

        return Task.FromResult(new DocumentResult
        {
            Content = Encoding.UTF8.GetBytes(sb.ToString()),
            ContentType = "text/csv",
            FileName = fileName
        });
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
