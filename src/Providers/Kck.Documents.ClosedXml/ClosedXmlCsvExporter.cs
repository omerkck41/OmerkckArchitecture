using System.Reflection;
using System.Text;
using Kck.Documents.Abstractions;

namespace Kck.Documents.ClosedXml;

public sealed class ClosedXmlCsvExporter : ICsvExporter
{
    public Task<DocumentResult> ExportAsync<T>(IEnumerable<T> data, string fileName = "export.csv", CancellationToken ct = default)
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
