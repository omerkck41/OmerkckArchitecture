using Kck.Documents.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Kck.Documents.ImageSharp;

/// <summary>
/// Implementation of <see cref="IImageProcessor"/> that uses the SixLabors ImageSharp library
/// to resize, convert and inspect image streams without native dependencies.
/// </summary>
public sealed class ImageSharpProcessor : IImageProcessor
{
    /// <summary>
    /// Resizes the image from <paramref name="input"/> according to <paramref name="options"/>
    /// and returns the encoded bytes in the requested output format.
    /// </summary>
    public async Task<byte[]> ResizeAsync(Stream input, ImageProcessingOptions options, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(input, ct).ConfigureAwait(false);

        var width = options.Width ?? image.Width;
        var height = options.Height ?? image.Height;

        image.Mutate(x =>
        {
            if (options.MaintainAspectRatio)
                x.Resize(new ResizeOptions { Size = new Size(width, height), Mode = ResizeMode.Max });
            else
                x.Resize(width, height);
        });

        using var ms = new MemoryStream();
        var encoder = GetEncoder(options.OutputFormat, image, options.Quality);
        await image.SaveAsync(ms, encoder, ct).ConfigureAwait(false);
        return ms.ToArray();
    }

    /// <summary>
    /// Decodes the image from <paramref name="input"/> and re-encodes it to the specified
    /// <paramref name="targetFormat"/> (jpeg/jpg, png, webp, gif, bmp). An empty format keeps the source format.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="targetFormat"/> is not a supported format.</exception>
    public async Task<byte[]> ConvertFormatAsync(Stream input, string targetFormat, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(input, ct).ConfigureAwait(false);
        using var ms = new MemoryStream();
        var encoder = GetEncoder(targetFormat, image, 80);
        await image.SaveAsync(ms, encoder, ct).ConfigureAwait(false);
        return ms.ToArray();
    }

    /// <summary>
    /// Returns the pixel dimensions of the image contained in <paramref name="input"/> without
    /// fully decoding the pixel data.
    /// </summary>
    public async Task<(int Width, int Height)> GetDimensionsAsync(Stream input, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(input, ct).ConfigureAwait(false);
        return (image.Width, image.Height);
    }

    // An empty format keeps the source format (ImageProcessingOptions.OutputFormat contract); a source format Kck does
    // not encode (e.g. TIFF) falls back to JPEG. A requested name that is not supported is an error rather than a
    // silent JPEG, so a typo or an unsupported format (e.g. "avif") is noticed.
    private static IImageEncoder GetEncoder(string? format, Image source, int quality)
    {
        if (string.IsNullOrWhiteSpace(format))
            return CreateEncoder(source.Metadata.DecodedImageFormat?.Name, quality) ?? new JpegEncoder { Quality = quality };

        return CreateEncoder(format, quality)
               ?? throw new NotSupportedException(
                   $"Image format '{format}' is not supported. Supported formats: jpeg (jpg), png, webp, gif, bmp.");
    }

    private static IImageEncoder? CreateEncoder(string? format, int quality) =>
        format?.Trim().ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => new JpegEncoder { Quality = quality },
            "png" => new PngEncoder(),
            "gif" => new GifEncoder(),
            "bmp" => new BmpEncoder(),
            "webp" => new WebpEncoder { Quality = quality },
            _ => null
        };
}
