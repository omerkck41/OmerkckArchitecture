using Kck.Documents.Abstractions;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Kck.Documents.SkiaSharp;

/// <summary>
/// Implementation of <see cref="IImageProcessor"/> on SkiaSharp. Reads JPEG, PNG, WebP, GIF, BMP and ICO; writes
/// JPEG, PNG and WebP. On Linux the application must reference <c>SkiaSharp.NativeAssets.Linux.NoDependencies</c>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>A source or resize target larger than <see cref="SkiaSharpOptions.MaxPixels"/> is rejected from the header,
/// before any pixel is decoded.</item>
/// <item>When the output is JPEG and the source has an alpha channel, transparent areas become white.</item>
/// <item>EXIF orientation is applied (baked into the pixels). The output carries no metadata: EXIF (including GPS) and
/// the ICC profile are not copied; pixels are converted to sRGB.</item>
/// <item>Truncated or corrupt image data throws <see cref="InvalidDataException"/>.</item>
/// </list>
/// </remarks>
public sealed class SkiaSharpImageProcessor : IImageProcessor
{
    private const int ConvertQuality = 80;
    private const string SupportedOutputFormats = "jpeg (jpg), png, webp";

    // Linear filtering with linear mipmaps: good for the common case (thumbnails, downscaling) without the aliasing a
    // plain bilinear or cubic filter shows at large reduction ratios.
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    private readonly int _maxPixels;

    /// <summary>Creates a processor with the default <see cref="SkiaSharpOptions"/>.</summary>
    public SkiaSharpImageProcessor() : this(Options.Create(new SkiaSharpOptions())) { }

    /// <summary>Creates a processor with the given options.</summary>
    /// <param name="options">The processor options.</param>
    public SkiaSharpImageProcessor(IOptions<SkiaSharpOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _maxPixels = options.Value.MaxPixels;
    }

    /// <summary>
    /// Resizes the image from <paramref name="input"/> according to <paramref name="options"/> and returns the encoded
    /// bytes. When only one of Width/Height is set (the other <c>null</c> or <c>0</c>), the other follows the source
    /// ratio; with neither set the size is kept. With both set and
    /// <see cref="ImageProcessingOptions.MaintainAspectRatio"/> the image is scaled to fit inside the box (up or down);
    /// otherwise it is stretched to the exact size. <see cref="ImageProcessingOptions.Quality"/> is clamped to 1..100.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Width or Height is negative.</exception>
    /// <exception cref="InvalidDataException">The stream is not an image SkiaSharp can decode, or its data is truncated
    /// or corrupt.</exception>
    /// <exception cref="InvalidOperationException">The image or the requested size exceeds
    /// <see cref="SkiaSharpOptions.MaxPixels"/>.</exception>
    /// <exception cref="NotSupportedException">The output format is not jpeg (jpg), png or webp.</exception>
    public async Task<byte[]> ResizeAsync(Stream input, ImageProcessingOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(options);
        if (options.Width < 0 || options.Height < 0)
            throw new ArgumentOutOfRangeException(
                nameof(options), $"Width and Height must not be negative (got {options.Width}x{options.Height}).");

        using var buffered = await BufferIfNotSeekableAsync(input, ct).ConfigureAwait(false);
        return Process(buffered ?? input, options.OutputFormat, options.Quality, options, ct);
    }

    /// <summary>
    /// Decodes the image from <paramref name="input"/> and re-encodes it to <paramref name="targetFormat"/>
    /// (jpeg/jpg, png, webp) with quality 80. An empty format keeps the source format (JPEG when the source is GIF,
    /// BMP or ICO).
    /// </summary>
    /// <exception cref="InvalidDataException">The stream is not an image SkiaSharp can decode, or its data is truncated
    /// or corrupt.</exception>
    /// <exception cref="InvalidOperationException">The image exceeds <see cref="SkiaSharpOptions.MaxPixels"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="targetFormat"/> is not jpeg (jpg), png or webp.</exception>
    public async Task<byte[]> ConvertFormatAsync(Stream input, string targetFormat, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var buffered = await BufferIfNotSeekableAsync(input, ct).ConfigureAwait(false);
        return Process(buffered ?? input, targetFormat, ConvertQuality, resize: null, ct);
    }

    /// <summary>
    /// Returns the pixel dimensions as displayed: EXIF orientation applied (the same size <see cref="ResizeAsync"/>
    /// starts from), still read from the header without decoding pixels. The <see cref="SkiaSharpOptions.MaxPixels"/> limit does not apply here (only the header is read, no pixels
    /// are decoded), so this method can be used to check an image before processing it. A non-seekable stream is fully
    /// buffered in memory first.
    /// </summary>
    /// <exception cref="InvalidDataException">The stream is not an image SkiaSharp can decode.</exception>
    public async Task<(int Width, int Height)> GetDimensionsAsync(Stream input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var buffered = await BufferIfNotSeekableAsync(input, ct).ConfigureAwait(false);
        using var skStream = new SKManagedStream(buffered ?? input, disposeManagedStream: false);
        using var codec = CreateCodec(skStream);
        return SwapsDimensions(codec.EncodedOrigin)
            ? (codec.Info.Height, codec.Info.Width)
            : (codec.Info.Width, codec.Info.Height);
    }

    private byte[] Process(
        Stream input, string? format, int quality, ImageProcessingOptions? resize, CancellationToken ct)
    {
        // The managed stream must outlive the codec (declared first, disposed last); the caller's stream stays open.
        using var skStream = new SKManagedStream(input, disposeManagedStream: false);
        using var codec = CreateCodec(skStream);

        // Everything that can reject the request is checked from the header, before any pixel is decoded.
        var info = codec.Info;
        if ((long)info.Width * info.Height > _maxPixels)
            throw new InvalidOperationException(
                $"The image is {info.Width}x{info.Height} ({(long)info.Width * info.Height} pixels), which exceeds the " +
                $"SkiaSharpOptions.MaxPixels limit of {_maxPixels}.");

        var origin = codec.EncodedOrigin;
        var (uprightWidth, uprightHeight) = SwapsDimensions(origin) ? (info.Height, info.Width) : (info.Width, info.Height);
        var size = resize is null ? null : TargetSize(uprightWidth, uprightHeight, resize);
        if (size is var (targetWidth, targetHeight) && (long)targetWidth * targetHeight > _maxPixels)
            throw new InvalidOperationException(
                $"The requested size {targetWidth}x{targetHeight} ({(long)targetWidth * targetHeight} pixels) exceeds " +
                $"SkiaSharpOptions.MaxPixels ({_maxPixels}).");

        var target = ResolveFormat(format, codec.EncodedFormat);

        ct.ThrowIfCancellationRequested();

        // At most two full bitmaps are alive at a time: each step disposes its input once its output exists.
        var bitmap = Decode(codec);
        try
        {
            bitmap = Replace(bitmap, Orient(bitmap, origin));
            if (size is var (width, height))
                bitmap = Replace(bitmap, Resize(bitmap, width, height));
            if (target == SKEncodedImageFormat.Jpeg && bitmap.AlphaType != SKAlphaType.Opaque)
                bitmap = Replace(bitmap, FlattenOnWhite(bitmap));

            ct.ThrowIfCancellationRequested();
            return Encode(bitmap, target, Math.Clamp(quality, 1, 100));
        }
        finally
        {
            bitmap.Dispose();
        }
    }

    private static SKBitmap Replace(SKBitmap current, SKBitmap? next)
    {
        if (next is null)
            return current;

        current.Dispose();
        return next;
    }

    private static SKCodec CreateCodec(SKStream stream) =>
        SKCodec.Create(stream)
        ?? throw new InvalidDataException(
            "The stream is not an image SkiaSharp can decode (supported: JPEG, PNG, WebP, GIF, BMP, ICO).");

    // Strict decode: a truncated or corrupt stream is an error instead of a partially filled image. The codec converts
    // the pixels to sRGB (an embedded Display P3 / Adobe RGB profile is applied); the bitmap itself carries no colour
    // space, so the encoders write no profile. Unpremultiplied alpha is decoded premultiplied, as SKBitmap.Decode
    // does, so drawing and resampling blend correctly.
    private static SKBitmap Decode(SKCodec codec)
    {
        var info = codec.Info;
        var colorType = info.ColorType == SKColorType.Unknown ? SKImageInfo.PlatformColorType : info.ColorType;
        var alphaType = info.AlphaType is SKAlphaType.Unknown or SKAlphaType.Unpremul ? SKAlphaType.Premul : info.AlphaType;
        var bitmapInfo = new SKImageInfo(info.Width, info.Height, colorType, alphaType);

        var bitmap = new SKBitmap(bitmapInfo);
        var result = codec.GetPixels(bitmapInfo.WithColorSpace(Srgb), bitmap.GetPixels());
        if (result != SKCodecResult.Success)
        {
            bitmap.Dispose();
            throw new InvalidDataException($"The image data is truncated or corrupt (codec result: {result}).");
        }

        return bitmap;
    }

    // SKEncodedOrigin follows EXIF Orientation 1..8; LeftTop, RightTop, RightBottom and LeftBottom are rotated by 90°.
    private static bool SwapsDimensions(SKEncodedOrigin origin) =>
        origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom
            or SKEncodedOrigin.LeftBottom;

    // Returns null for TopLeft (already upright). The matrix maps a stored pixel (x, y) of a w x h image to its upright
    // position; whole-pixel mappings with nearest sampling, so no pixel is resampled.
    private static SKBitmap? Orient(SKBitmap source, SKEncodedOrigin origin)
    {
        float w = source.Width, h = source.Height;
        SKMatrix? matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),     // mirror horizontally
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1), // rotate 180°
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),   // mirror vertically
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),       // transpose
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),     // rotate 90° clockwise
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1), // transverse
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),   // rotate 90° counter-clockwise
            _ => null
        };
        if (matrix is null)
            return null;

        var (width, height) = SwapsDimensions(origin) ? (source.Height, source.Width) : (source.Width, source.Height);
        var oriented = new SKBitmap(new SKImageInfo(width, height, source.ColorType, source.AlphaType));
        using var canvas = new SKCanvas(oriented);
        canvas.Clear(SKColors.Transparent);
        canvas.SetMatrix(matrix.Value);
        canvas.DrawBitmap(source, 0, 0, SKSamplingOptions.Default, paint: null);
        return oriented;
    }

    // Returns null when the size does not change (no resampling needed). Negative values were rejected by ResizeAsync.
    // Neither side given (null or 0): source size. One side given: the other follows the source ratio, whatever
    // MaintainAspectRatio says (a single side cannot distort). Both given: fit inside the box or stretch.
    private static (int Width, int Height)? TargetSize(int sourceWidth, int sourceHeight, ImageProcessingOptions options)
    {
        var requestedWidth = options.Width ?? 0;
        var requestedHeight = options.Height ?? 0;

        int width, height;
        if (requestedWidth == 0 && requestedHeight == 0)
            return null;

        if (requestedHeight == 0)
        {
            width = requestedWidth;
            height = Math.Max(1, (int)Math.Round((double)requestedWidth * sourceHeight / sourceWidth));
        }
        else if (requestedWidth == 0)
        {
            width = Math.Max(1, (int)Math.Round((double)requestedHeight * sourceWidth / sourceHeight));
            height = requestedHeight;
        }
        else if (options.MaintainAspectRatio)
        {
            // Fit inside the box keeping the ratio (ImageSharp ResizeMode.Max): 200x100 into 100x100 -> 100x50.
            var scale = Math.Min((double)requestedWidth / sourceWidth, (double)requestedHeight / sourceHeight);
            width = Math.Max(1, (int)Math.Round(sourceWidth * scale));
            height = Math.Max(1, (int)Math.Round(sourceHeight * scale));
        }
        else
        {
            width = requestedWidth;
            height = requestedHeight;
        }

        return width == sourceWidth && height == sourceHeight ? null : (width, height);
    }

    private static SKBitmap Resize(SKBitmap source, int width, int height) =>
        source.Resize(source.Info.WithSize(width, height), Sampling)
        ?? throw new InvalidOperationException($"SkiaSharp could not resize the image to {width}x{height}.");

    // JPEG has no alpha channel: draw on white so transparent areas do not turn black.
    private static SKBitmap FlattenOnWhite(SKBitmap source)
    {
        var flattened = new SKBitmap(
            new SKImageInfo(source.Width, source.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque));
        using var canvas = new SKCanvas(flattened);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(source, 0, 0, SKSamplingOptions.Default, paint: null);
        return flattened;
    }

    private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format, int quality)
    {
        using var data = bitmap.Encode(format, quality)
                         ?? throw new InvalidOperationException($"SkiaSharp could not encode the image as {format}.");
        return data.ToArray();
    }

    // An empty format keeps the source format (ImageProcessingOptions.OutputFormat contract); a decodable source Kck
    // cannot encode (GIF, BMP, ICO, WBMP) falls back to JPEG. A named format that is not supported is an error rather
    // than a silent JPEG, so a typo or an unsupported format (e.g. "gif", "avif") is noticed.
    private static SKEncodedImageFormat ResolveFormat(string? format, SKEncodedImageFormat source)
    {
        if (string.IsNullOrWhiteSpace(format))
            return source is SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp
                ? source
                : SKEncodedImageFormat.Jpeg;

        return format.Trim().ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => SKEncodedImageFormat.Jpeg,
            "png" => SKEncodedImageFormat.Png,
            "webp" => SKEncodedImageFormat.Webp,
            _ => throw new NotSupportedException(
                $"Image format '{format}' is not supported. Supported formats: {SupportedOutputFormats}.")
        };
    }

    // SkiaSharp reads synchronously; a forward-only stream (e.g. an HTTP request body) is copied asynchronously first.
    private static async Task<MemoryStream?> BufferIfNotSeekableAsync(Stream input, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (input.CanSeek)
            return null;

        var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, ct).ConfigureAwait(false);
        buffer.Position = 0;
        return buffer;
    }
}
