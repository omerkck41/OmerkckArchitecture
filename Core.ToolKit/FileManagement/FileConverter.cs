namespace Core.ToolKit.FileManagement;

public static class FileConverter
{
    /// <summary>
    /// Converts a file to a Base64 encoded string asynchronously.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>Base64 encoded string.</returns>
    public static async Task<string> ToBase64Async(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
        return Convert.ToBase64String(fileBytes);
    }

    /// <summary>
    /// Creates a file from a Base64 encoded string asynchronously.
    /// </summary>
    /// <param name="base64String">Base64 encoded string.</param>
    /// <param name="outputPath">The path where the file will be saved.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task FromBase64Async(string base64String, string outputPath)
    {
        if (string.IsNullOrEmpty(base64String))
            throw new ArgumentNullException(nameof(base64String), "Base64 string cannot be null or empty.");

        byte[] fileBytes = Convert.FromBase64String(base64String);
        await File.WriteAllBytesAsync(outputPath, fileBytes);
    }
}