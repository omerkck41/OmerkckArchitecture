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

public sealed class ImageSharpProcessor : IImageProcessor
{
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
        var encoder = GetEncoder(options.OutputFormat, options.Quality);
        await image.SaveAsync(ms, encoder, ct).ConfigureAwait(false);
        return ms.ToArray();
    }

    public async Task<byte[]> ConvertFormatAsync(Stream input, string targetFormat, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(input, ct).ConfigureAwait(false);
        using var ms = new MemoryStream();
        var encoder = GetEncoder(targetFormat, 80);
        await image.SaveAsync(ms, encoder, ct).ConfigureAwait(false);
        return ms.ToArray();
    }

    public async Task<(int Width, int Height)> GetDimensionsAsync(Stream input, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(input, ct).ConfigureAwait(false);
        return (image.Width, image.Height);
    }

    private static IImageEncoder GetEncoder(string? format, int quality) =>
        format?.ToLowerInvariant() switch
        {
            "png" => new PngEncoder(),
            "gif" => new GifEncoder(),
            "bmp" => new BmpEncoder(),
            "webp" => new WebpEncoder { Quality = quality },
            _ => new JpegEncoder { Quality = quality }
        };
}
