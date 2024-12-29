using System.Text.Json;

namespace Core.ToolKit.DataProcessing;

public static class JsonHelper
{
    /// <summary>
    /// Reads a JSON file and deserializes it into a specified object type.
    /// Includes detailed validation and error handling.
    /// </summary>
    /// <typeparam name="T">Type of the object to deserialize to.</typeparam>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <returns>Deserialized object of type T.</returns>
    public static async Task<T> ReadJsonAsync<T>(string filePath) where T : class
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"JSON file not found at path: {filePath}");

        try
        {
            var jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            }) ?? throw new InvalidOperationException("Failed to deserialize JSON content.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON format.", ex);
        }
    }

    /// <summary>
    /// Serializes an object to a JSON string and writes it to a file with advanced formatting options.
    /// </summary>
    /// <param name="data">Object to serialize.</param>
    /// <param name="filePath">Path to save the JSON file.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task WriteJsonAsync(object data, string filePath, bool indented = true)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data), "Data to write cannot be null.");

        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");

        try
        {
            var options = new JsonSerializerOptions { WriteIndented = indented };
            var jsonString = JsonSerializer.Serialize(data, options);

            await File.WriteAllTextAsync(filePath, jsonString);
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to write JSON to file.", ex);
        }
    }
}