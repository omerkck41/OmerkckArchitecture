namespace Kck.Documents.Abstractions;

/// <summary>
/// Specifies options for resizing and encoding an image, including target dimensions, aspect ratio preservation, output format, and quality.
/// </summary>
public sealed class ImageProcessingOptions
{
    /// <summary>Gets the target width in pixels, or <see langword="null"/> to derive it from height.</summary>
    public int? Width { get; init; }

    /// <summary>Gets the target height in pixels, or <see langword="null"/> to derive it from width.</summary>
    public int? Height { get; init; }

    /// <summary>Gets a value indicating whether the original aspect ratio is preserved during resize.</summary>
    public bool MaintainAspectRatio { get; init; } = true;

    /// <summary>Gets the desired output format (e.g. "jpeg", "png", "webp"), or <see langword="null"/> to keep the source format.</summary>
    public string? OutputFormat { get; init; }

    /// <summary>Gets the compression quality level from 1 (lowest) to 100 (highest). Defaults to 80.</summary>
    public int Quality { get; init; } = 80;
}
