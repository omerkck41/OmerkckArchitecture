using System.Dynamic;

namespace Core.ToolKit.DataProcessing;

public static class CsvHelper
{
    /// <summary>
    /// Reads a CSV file and converts it into a list of dynamic objects with proper parsing and error handling.
    /// </summary>
    /// <param name="filePath">Path to the CSV file.</param>
    /// <param name="delimiter">Delimiter used in the CSV file.</param>
    /// <returns>List of dynamic objects representing rows of the CSV file.</returns>
    public static async Task<List<ExpandoObject>> ReadCsvAsync(string filePath, char delimiter = ',')
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"CSV file not found at path: {filePath}");

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                throw new InvalidOperationException("CSV file is empty.");

            var headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();
            var rows = new List<ExpandoObject>();

            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(delimiter);
                var row = new ExpandoObject() as IDictionary<string, object>;

                for (int i = 0; i < headers.Length; i++)
                {
                    row[headers[i]] = i < values.Length && values[i] != null ? values[i].Trim() : string.Empty;
                }

                rows.Add((ExpandoObject)row!);
            }

            return rows;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error occurred while reading CSV file.", ex);
        }
    }

    /// <summary>
    /// Writes a list of objects to a CSV file with proper formatting and delimiter options.
    /// </summary>
    /// <typeparam name="T">Type of objects to write.</typeparam>
    /// <param name="data">List of objects to write.</param>
    /// <param name="filePath">Path to save the CSV file.</param>
    /// <param name="delimiter">Delimiter to use in the CSV file.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task WriteCsvAsync<T>(IEnumerable<T> data, string filePath, char delimiter = ',')
    {
        if (data == null || !data.Any())
            throw new ArgumentNullException(nameof(data), "Data cannot be null or empty.");

        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");

        try
        {
            using var writer = new StreamWriter(filePath);

            var properties = typeof(T).GetProperties();
            await writer.WriteLineAsync(string.Join(delimiter, properties.Select(p => p.Name)));

            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString()?.Replace(delimiter.ToString(), string.Empty) ?? string.Empty);
                await writer.WriteLineAsync(string.Join(delimiter, values));
            }
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to write CSV file.", ex);
        }
    }
}