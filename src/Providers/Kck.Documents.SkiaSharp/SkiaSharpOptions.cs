namespace Kck.Documents.SkiaSharp;

/// <summary>
/// Options for <see cref="SkiaSharpImageProcessor"/>.
/// </summary>
public sealed class SkiaSharpOptions
{
    /// <summary>
    /// Gets or sets the largest image, in pixels (width × height), the processor accepts. It applies to the source
    /// image and to the resize target. Both are checked from the image header before any pixel is decoded, so an
    /// oversized image (a decoder bomb) or an oversized request is rejected with an
    /// <see cref="InvalidOperationException"/> without decoding its pixels. Defaults to 50,000,000 (for example
    /// 8000 × 6250); must be at least 1.
    /// </summary>
    public int MaxPixels { get; set; } = 50_000_000;
}
