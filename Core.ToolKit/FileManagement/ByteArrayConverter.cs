namespace Core.ToolKit.FileManagement;

public static class ByteArrayConverter
{
    /// <summary>
    /// Reads a file and converts it to a byte array asynchronously.
    /// </summary>
    /// <param name="filePath">The full path of the file.</param>
    /// <returns>Byte array of the file content.</returns>
    public static async Task<byte[]> ToByteArrayAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found at path: {filePath}");

        return await File.ReadAllBytesAsync(filePath);
    }

    /// <summary>
    /// Writes a byte array to a file asynchronously.
    /// </summary>
    /// <param name="data">The byte array content.</param>
    /// <param name="outputPath">The path where the file will be saved.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task FromByteArrayAsync(byte[] data, string outputPath)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentNullException(nameof(data), "Byte array cannot be null or empty.");

        await File.WriteAllBytesAsync(outputPath, data);
    }
}