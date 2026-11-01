using AwesomeAssertions;
using Kck.Documents.Abstractions;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Xunit;

namespace Kck.Documents.SkiaSharp.Tests;

public class SkiaSharpImageProcessorTests
{
    private readonly SkiaSharpImageProcessor _sut = new();

    private static SKBitmap Decode(byte[] bytes) =>
        SKBitmap.Decode(bytes) ?? throw new InvalidOperationException("Result is not a decodable image.");

    [Fact]
    public async Task GetDimensionsAsync_ShouldReturnOriginalDimensions()
    {
        using var input = TestImages.Png(320, 240);

        var (w, h) = await _sut.GetDimensionsAsync(input);

        w.Should().Be(320);
        h.Should().Be(240);
    }

    // Ö7: the IHDR claims 8000×8000 but the pixel data is a 4×4 image; only a header-only read returns the claim.
    [Fact]
    public async Task GetDimensionsAsync_ReadsHeaderWithoutDecoding()
    {
        using var input = TestImages.PngWithPatchedHeader(8000, 8000);

        var (w, h) = await _sut.GetDimensionsAsync(input);

        w.Should().Be(8000);
        h.Should().Be(8000);
    }

    [Fact]
    public async Task GetDimensionsAsync_NonSeekableStream_ReturnsDimensions()
    {
        using var inner = TestImages.Png(30, 20);
        using var input = new NonSeekableStream(inner);

        var (w, h) = await _sut.GetDimensionsAsync(input);

        w.Should().Be(30);
        h.Should().Be(20);
    }

    [Fact]
    public async Task ResizeAsync_WithAspectRatio_ShouldFitWithinBox()
    {
        using var input = TestImages.Png(200, 100);
        var options = new ImageProcessingOptions
        {
            Width = 100,
            Height = 100,
            MaintainAspectRatio = true,
            OutputFormat = "png"
        };

        var bytes = await _sut.ResizeAsync(input, options);

        bytes.Should().NotBeEmpty();
        using var result = Decode(bytes);
        result.Width.Should().Be(100);
        result.Height.Should().Be(50);
    }

    // Parity with ImageSharp ResizeMode.Max: a smaller image is scaled up until it touches the box.
    [Fact]
    public async Task ResizeAsync_WithAspectRatio_SmallerSource_UpscalesToFitBox()
    {
        using var input = TestImages.Png(50, 25);
        var options = new ImageProcessingOptions { Width = 100, Height = 100, OutputFormat = "png" };

        var bytes = await _sut.ResizeAsync(input, options);

        using var result = Decode(bytes);
        result.Width.Should().Be(100);
        result.Height.Should().Be(50);
    }

    [Fact]
    public async Task ResizeAsync_WithoutAspectRatio_ShouldForceExactDimensions()
    {
        using var input = TestImages.Png(200, 100);
        var options = new ImageProcessingOptions
        {
            Width = 50,
            Height = 50,
            MaintainAspectRatio = false,
            OutputFormat = "png"
        };

        var bytes = await _sut.ResizeAsync(input, options);

        using var result = Decode(bytes);
        result.Width.Should().Be(50);
        result.Height.Should().Be(50);
    }

    [Theory]
    [InlineData("png")]
    [InlineData("jpeg")]
    [InlineData("webp")]
    public async Task ConvertFormatAsync_ShouldProduceLoadableBytes(string format)
    {
        using var input = TestImages.Png(40, 30);

        var bytes = await _sut.ConvertFormatAsync(input, format);

        bytes.Should().NotBeEmpty();
        using var loaded = Decode(bytes);
        loaded.Width.Should().Be(40);
        loaded.Height.Should().Be(30);
    }

    // D1(a): SkiaSharp cannot encode GIF or BMP.
    [Theory]
    [InlineData("gif")]
    [InlineData("bmp")]
    public async Task ConvertFormatAsync_GifOrBmp_ThrowsNotSupported(string format)
    {
        using var input = TestImages.Png(20, 20);

        var act = () => _sut.ConvertFormatAsync(input, format);

        (await act.Should().ThrowAsync<NotSupportedException>())
            .WithMessage($"*'{format}'*")
            .And.Message.Should().Contain("jpeg (jpg), png, webp");
    }

    [Theory]
    [InlineData("avif")]
    [InlineData("unknown-xyz")]
    public async Task ConvertFormatAsync_UnknownFormat_ThrowsAndListsSupportedFormats(string format)
    {
        using var input = TestImages.Png(20, 20);

        var act = () => _sut.ConvertFormatAsync(input, format);

        (await act.Should().ThrowAsync<NotSupportedException>())
            .WithMessage($"*{format}*")
            .And.Message.Should().ContainAll("jpeg", "png", "webp");
    }

    [Theory]
    [InlineData("jpg")]
    [InlineData("jpeg")]
    [InlineData("JPEG")]
    [InlineData(" jpg ")]
    public async Task ConvertFormatAsync_JpegAliases_ProduceJpeg(string format)
    {
        using var input = TestImages.Png(20, 20);

        var bytes = await _sut.ConvertFormatAsync(input, format);

        bytes.Take(2).Should().Equal(0xFF, 0xD8);
    }

    // Ö3
    [Fact]
    public async Task ConvertFormatAsync_Webp_ProducesWebpSignature()
    {
        using var input = TestImages.Png(20, 20);

        var bytes = await _sut.ConvertFormatAsync(input, "WebP");

        bytes.Take(4).Should().Equal("RIFF"u8.ToArray());
        bytes.Skip(8).Take(4).Should().Equal("WEBP"u8.ToArray());
    }

    [Fact]
    public async Task ResizeAsync_WithoutOutputFormat_KeepsSourceFormat()
    {
        using var input = TestImages.Png(20, 20);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        bytes.Take(4).Should().Equal(TestImages.PngSignature);
    }

    [Fact]
    public async Task ResizeAsync_WithoutOutputFormat_WebpSource_KeepsWebp()
    {
        using var input = new MemoryStream(TestImages.Encode(20, 20, SKEncodedImageFormat.Webp));

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        bytes.Skip(8).Take(4).Should().Equal("WEBP"u8.ToArray());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ConvertFormatAsync_EmptyFormat_KeepsSourceFormat(string format)
    {
        using var input = TestImages.Png(20, 20);

        var bytes = await _sut.ConvertFormatAsync(input, format);

        bytes.Take(4).Should().Equal(TestImages.PngSignature);
    }

    [Fact]
    public async Task ResizeAsync_WithoutOutputFormat_GifSource_ProducesJpeg()
    {
        using var input = TestImages.Gif1x1();

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        bytes.Take(2).Should().Equal(0xFF, 0xD8);
        using var result = Decode(bytes);
        result.Width.Should().Be(10);
        result.Height.Should().Be(10);
    }

    [Fact]
    public async Task ResizeAsync_WithoutOutputFormat_BmpSource_ProducesJpeg()
    {
        using var input = TestImages.Bmp24(4, 2);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        bytes.Take(2).Should().Equal(0xFF, 0xD8);
        using var result = Decode(bytes);
        result.Width.Should().Be(10);
        result.Height.Should().Be(5);
    }

    [Fact]
    public async Task ResizeAsync_UnsupportedSourceFormat_ThrowsInvalidData()
    {
        // TIFF little-endian header followed by junk: SkiaSharp has no TIFF decoder.
        byte[] tiff = [0x49, 0x49, 0x2A, 0x00, .. Enumerable.Range(0, 60).Select(i => (byte)(i * 7))];
        using var input = new MemoryStream(tiff);

        var act = () => _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        (await act.Should().ThrowAsync<InvalidDataException>())
            .Which.Message.Should().Contain("JPEG, PNG, WebP, GIF, BMP, ICO");
    }

    [Fact]
    public async Task ResizeAsync_NullDimensions_ShouldPreserveOriginal()
    {
        using var input = TestImages.Png(64, 48);
        var options = new ImageProcessingOptions
        {
            Width = null,
            Height = null,
            OutputFormat = "png"
        };

        var bytes = await _sut.ResizeAsync(input, options);

        using var result = Decode(bytes);
        result.Width.Should().Be(64);
        result.Height.Should().Be(48);
    }

    [Theory]
    [InlineData(10, -1)]
    [InlineData(-5, 10)]
    public async Task ResizeAsync_NegativeDimension_ThrowsArgumentOutOfRange(int width, int height)
    {
        using var input = TestImages.Png(20, 20);

        var act = () => _sut.ResizeAsync(input, new ImageProcessingOptions { Width = width, Height = height });

        (await act.Should().ThrowAsync<ArgumentOutOfRangeException>())
            .WithMessage($"*{width}x{height}*");
    }

    // Negative sizes are rejected before the stream is read (no buffering of a request body for an invalid request).
    [Fact]
    public async Task ResizeAsync_NegativeDimension_ThrowsBeforeReadingStream()
    {
        using var input = new UnreadableStream();

        var act = () => _sut.ResizeAsync(input, new ImageProcessingOptions { Width = -1 });

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // ImageProcessingOptions contract (and ImageSharp parity): a single side given, the other (null or 0) is derived
    // from the source ratio, whatever MaintainAspectRatio says; 200x100 source.
    [Theory]
    [InlineData(0, 50, true, 100, 50)]
    [InlineData(0, 50, false, 100, 50)]
    [InlineData(null, 400, true, 800, 400)]
    [InlineData(100, null, true, 100, 50)]
    [InlineData(100, 0, false, 100, 50)]
    public async Task ResizeAsync_SingleDimension_DerivesTheOtherFromSourceRatio(
        int? width, int? height, bool keepRatio, int expectedWidth, int expectedHeight)
    {
        using var input = TestImages.Png(200, 100);
        var options = new ImageProcessingOptions
        {
            Width = width,
            Height = height,
            MaintainAspectRatio = keepRatio,
            OutputFormat = "png"
        };

        var bytes = await _sut.ResizeAsync(input, options);

        using var result = Decode(bytes);
        result.Width.Should().Be(expectedWidth);
        result.Height.Should().Be(expectedHeight);
    }

    [Fact]
    public async Task ResizeAsync_DerivedDimensionExceedingMaxPixels_Throws()
    {
        var sut = new SkiaSharpImageProcessor(Options.Create(new SkiaSharpOptions { MaxPixels = 1000 }));
        using var input = TestImages.Png(20, 10);

        var act = () => sut.ResizeAsync(input, new ImageProcessingOptions { Height = 40, OutputFormat = "png" });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*80x40*MaxPixels*1000*");
    }

    // X1 / Ö9: EXIF orientation 6 (rotate 90° clockwise) is baked into the pixels.
    [Fact]
    public async Task ResizeAsync_Exif6Jpeg_IsOrientedUpright()
    {
        using var input = TestImages.JpegWithExifOrientation(40, 20, orientation: 6);

        var (displayedWidth, displayedHeight) = await _sut.GetDimensionsAsync(input);
        input.Position = 0;
        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { OutputFormat = "png" });

        displayedWidth.Should().Be(20);
        displayedHeight.Should().Be(40);
        using var result = Decode(bytes);
        result.Width.Should().Be(20);
        result.Height.Should().Be(40);
    }

    // X1: all eight EXIF orientations. Stored quadrants: R G / B Y; expected upright corners TL, TR, BL, BR.
    [Theory]
    [InlineData(1, "RGBY")]
    [InlineData(2, "GRYB")] // mirrored horizontally
    [InlineData(3, "YBGR")] // rotated 180°
    [InlineData(4, "BYRG")] // mirrored vertically
    [InlineData(5, "RBGY")] // transposed
    [InlineData(6, "BRYG")] // rotated 90° clockwise
    [InlineData(7, "YGBR")] // transversed
    [InlineData(8, "GYRB")] // rotated 90° counter-clockwise
    public async Task ResizeAsync_ExifOrientation_CornersAreUpright(ushort orientation, string expectedCorners)
    {
        using var input = TestImages.JpegWithExifOrientation(40, 20, orientation, quadrants: true);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { OutputFormat = "png" });

        using var result = Decode(bytes);
        var (w, h) = (result.Width, result.Height);
        (w, h).Should().Be(orientation >= 5 ? (20, 40) : (40, 20));
        string corners = new(
        [
            Classify(result.GetPixel(w / 4, h / 4)),
            Classify(result.GetPixel(w * 3 / 4, h / 4)),
            Classify(result.GetPixel(w / 4, h * 3 / 4)),
            Classify(result.GetPixel(w * 3 / 4, h * 3 / 4))
        ]);
        corners.Should().Be(expectedCorners);
    }

    [Fact]
    public async Task ResizeAsync_GrayscaleJpegWithExif6_IsOrientedUpright()
    {
        using var input = TestImages.GrayJpegWithExifOrientation(40, 20, orientation: 6);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, OutputFormat = "jpeg" });

        using var result = Decode(bytes);
        result.Width.Should().Be(10);
        result.Height.Should().Be(20);
    }

    // GetDimensionsAsync reports the displayed size (orientation applied), consistent with the ResizeAsync output.
    [Theory]
    [InlineData(1, 40, 20)]
    [InlineData(2, 40, 20)]
    [InlineData(3, 40, 20)]
    [InlineData(4, 40, 20)]
    [InlineData(5, 20, 40)]
    [InlineData(6, 20, 40)]
    [InlineData(7, 20, 40)]
    [InlineData(8, 20, 40)]
    public async Task GetDimensionsAsync_ExifOrientation_ReturnsDisplayedSize(
        ushort orientation, int expectedWidth, int expectedHeight)
    {
        using var input = TestImages.JpegWithExifOrientation(40, 20, orientation);

        var (w, h) = await _sut.GetDimensionsAsync(input);

        (w, h).Should().Be((expectedWidth, expectedHeight));
    }

    private static char Classify(SKColor c) =>
        (c.Red > 128, c.Green > 128, c.Blue > 128) switch
        {
            (true, true, false) => 'Y',
            (true, false, false) => 'R',
            (false, true, false) => 'G',
            (false, false, true) => 'B',
            _ => '?'
        };

    // X2: the resize target is limited like the source.
    [Fact]
    public async Task ResizeAsync_TargetExceedingMaxPixels_Throws()
    {
        var sut = new SkiaSharpImageProcessor(Options.Create(new SkiaSharpOptions { MaxPixels = 1000 }));
        using var input = TestImages.Png(2, 2);

        var act = () => sut.ResizeAsync(input, new ImageProcessingOptions { Width = 400, Height = 400, OutputFormat = "png" });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*400x400*MaxPixels*1000*");
    }

    [Fact]
    public async Task ResizeAsync_SourceAtExactlyMaxPixels_IsAccepted()
    {
        var sut = new SkiaSharpImageProcessor(Options.Create(new SkiaSharpOptions { MaxPixels = 1000 }));
        using var input = TestImages.Png(40, 25);

        var bytes = await sut.ResizeAsync(input, new ImageProcessingOptions { OutputFormat = "png" });

        using var result = Decode(bytes);
        result.Width.Should().Be(40);
        result.Height.Should().Be(25);
    }

    // X3: a truncated stream is an error, not a partially grey image.
    [Fact]
    public async Task ConvertFormatAsync_TruncatedPng_ThrowsInvalidData()
    {
        var png = TestImages.NoisyPng(64, 64);
        using var input = new MemoryStream(png[..(png.Length / 2)]);

        var act = () => _sut.ConvertFormatAsync(input, "png");

        (await act.Should().ThrowAsync<InvalidDataException>())
            .WithMessage("*truncated or corrupt*");
    }

    // X6: out-of-range quality is clamped to 1..100.
    [Theory]
    [InlineData(150)]
    [InlineData(-5)]
    public async Task ResizeAsync_OutOfRangeQuality_IsClamped(int quality)
    {
        using var input = TestImages.Png(20, 20);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { OutputFormat = "jpeg", Quality = quality });

        bytes.Take(2).Should().Equal(0xFF, 0xD8);
    }

    [Fact]
    public async Task ResizeAsync_Quality10_ProducesSmallerJpegThanQuality95()
    {
        var png = TestImages.NoisyPng(64, 64);

        var low = await _sut.ResizeAsync(new MemoryStream(png), new ImageProcessingOptions { OutputFormat = "jpeg", Quality = 10 });
        var high = await _sut.ResizeAsync(new MemoryStream(png), new ImageProcessingOptions { OutputFormat = "jpeg", Quality = 95 });

        low.Length.Should().BeLessThan(high.Length);
    }

    // A token cancelled before the call is rejected up front, before the stream is read.
    [Fact]
    public async Task ResizeAsync_CancelledToken_Throws()
    {
        using var input = TestImages.Png(20, 20);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // X7: cancellation that arrives while the header is read (seekable stream, no buffering) is honoured by the
    // processing itself before the pixels are decoded.
    [Fact]
    public async Task ResizeAsync_CancelledWhileReadingHeader_Throws()
    {
        using var cts = new CancellationTokenSource();
        using var input = new CancelOnReadStream(TestImages.Png(20, 20), cts);

        var act = () => _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // X5: an embedded Display P3 profile is applied: the pixels are converted to sRGB (expected value computed by Skia).
    [Fact]
    public async Task ConvertFormatAsync_DisplayP3Png_IsConvertedToSrgb()
    {
        byte[] p3Pixel = [200, 100, 50, 255];
        var (png, expected) = TestImages.DisplayP3Png(p3Pixel);
        System.Text.Encoding.ASCII.GetString(png).Should().Contain("iCCP", "the source must carry the P3 profile");

        var bytes = await _sut.ConvertFormatAsync(new MemoryStream(png), "png");

        using var result = Decode(bytes);
        var actual = result.GetPixel(1, 1);
        ((int)actual.Red).Should().BeCloseTo(expected.Red, 2);
        ((int)actual.Green).Should().BeCloseTo(expected.Green, 2);
        ((int)actual.Blue).Should().BeCloseTo(expected.Blue, 2);
        new[] { actual.Red, actual.Green, actual.Blue }.Should().NotEqual(p3Pixel[..3], "P3 values differ in sRGB");
    }

    [Fact]
    public async Task ResizeAsync_ExtremeAspectRatio_KeepsAtLeastOnePixel()
    {
        using var input = TestImages.Png(1, 1000);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 100, Height = 100, OutputFormat = "png" });

        using var result = Decode(bytes);
        result.Width.Should().Be(1);
        result.Height.Should().Be(100);
    }

    [Fact]
    public async Task ConvertFormatAsync_IcoInput_ProducesJpeg()
    {
        using var input = TestImages.IcoWithPng(16);

        var bytes = await _sut.ConvertFormatAsync(input, "");

        bytes.Take(2).Should().Equal(0xFF, 0xD8);
        using var result = Decode(bytes);
        result.Width.Should().Be(16);
        result.Height.Should().Be(16);
    }

    [Fact]
    public async Task ResizeAsync_NonSeekableStream_Works()
    {
        using var inner = TestImages.Png(40, 20);
        using var input = new NonSeekableStream(inner);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 20, Height = 20, OutputFormat = "png" });

        using var result = Decode(bytes);
        result.Width.Should().Be(20);
        result.Height.Should().Be(10);
    }

    // X5: pixels are converted to sRGB but no colour profile is written (documented "no metadata" output).
    [Theory]
    [InlineData("png", "iCCP")]
    [InlineData("jpeg", "ICC_PROFILE")]
    [InlineData("webp", "ICCP")]
    public async Task ConvertFormatAsync_Output_HasNoEmbeddedColorProfile(string format, string marker)
    {
        using var input = TestImages.Png(20, 20);

        var bytes = await _sut.ConvertFormatAsync(input, format);

        System.Text.Encoding.ASCII.GetString(bytes).Should().NotContain(marker);
    }

    // X9: checks the "WebP quality 100 is lossless" claim on opaque noise (no premultiplication loss).
    [Fact]
    public async Task ResizeAsync_WebpQuality100_RoundTrips()
    {
        using var source = TestImages.NoisyBitmap(16, 16);
        using var sourceData = source.Encode(SKEncodedImageFormat.Png, 100);
        using var input = new MemoryStream(sourceData.ToArray());

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { OutputFormat = "webp", Quality = 100 });

        using var result = Decode(bytes);
        var differing = 0;
        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 16; x++)
            {
                if (result.GetPixel(x, y) != source.GetPixel(x, y))
                    differing++;
            }
        }

        differing.Should().Be(0);
    }

    [Fact]
    public async Task ResizeAsync_ExceedingMaxPixels_Throws()
    {
        var sut = new SkiaSharpImageProcessor(Options.Create(new SkiaSharpOptions { MaxPixels = 1000 }));
        using var input = TestImages.Png(40, 30);

        var act = () => sut.ResizeAsync(input, new ImageProcessingOptions { OutputFormat = "png" });

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*40x30*1000*");
    }

    // Decoder bomb: the header claims 64 megapixels (above the 50 M default); rejected from the header alone.
    [Fact]
    public async Task ConvertFormatAsync_DecoderBombHeader_ThrowsBeforeDecoding()
    {
        using var input = TestImages.PngWithPatchedHeader(8000, 8000);

        var act = () => _sut.ConvertFormatAsync(input, "png");

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*8000x8000*50000000*");
    }

    // Ö8: transparent areas become white instead of black when the target has no alpha channel.
    [Fact]
    public async Task ResizeAsync_TransparentPngToJpeg_HasWhiteBackground()
    {
        using var input = TestImages.Png(4, 4, SKColors.Transparent);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { OutputFormat = "jpeg", Quality = 100 });

        using var result = Decode(bytes);
        var pixel = result.GetPixel(1, 1);
        pixel.Red.Should().BeGreaterThanOrEqualTo(240);
        pixel.Green.Should().BeGreaterThanOrEqualTo(240);
        pixel.Blue.Should().BeGreaterThanOrEqualTo(240);
    }

    [Fact]
    public async Task ResizeAsync_LeavesCallerStreamOpen()
    {
        using var input = TestImages.Png(20, 20);

        await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        input.CanRead.Should().BeTrue();
    }
}
