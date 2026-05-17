namespace Kck.Documents.Abstractions;

/// <summary>
/// Processes images (resize, crop, convert format).
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Resizes the image stream according to the specified options and returns the result as a byte array.
    /// </summary>
    Task<byte[]> ResizeAsync(Stream input, ImageProcessingOptions options, CancellationToken ct = default);
    /// <summary>
    /// Converts the image stream to the specified target format and returns the result as a byte array.
    /// </summary>
    Task<byte[]> ConvertFormatAsync(Stream input, string targetFormat, CancellationToken ct = default);
    /// <summary>
    /// Reads the width and height dimensions from the image stream without fully decoding it.
    /// </summary>
    Task<(int Width, int Height)> GetDimensionsAsync(Stream input, CancellationToken ct = default);
}
