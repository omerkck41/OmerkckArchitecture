namespace Kck.Documents.Abstractions;

/// <summary>
/// Processes images (resize, crop, convert format).
/// </summary>
public interface IImageProcessor
{
    Task<byte[]> ResizeAsync(Stream input, ImageProcessingOptions options, CancellationToken ct = default);
    Task<byte[]> ConvertFormatAsync(Stream input, string targetFormat, CancellationToken ct = default);
    Task<(int Width, int Height)> GetDimensionsAsync(Stream input, CancellationToken ct = default);
}
