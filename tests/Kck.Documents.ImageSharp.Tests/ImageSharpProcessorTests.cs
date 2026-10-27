using AwesomeAssertions;
using Kck.Documents.Abstractions;
using Kck.Documents.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Kck.Documents.ImageSharp.Tests;

public class ImageSharpProcessorTests
{
    private readonly ImageSharpProcessor _sut = new();

    private static MemoryStream CreatePng(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task GetDimensionsAsync_ShouldReturnOriginalDimensions()
    {
        using var input = CreatePng(320, 240);

        var (w, h) = await _sut.GetDimensionsAsync(input);

        w.Should().Be(320);
        h.Should().Be(240);
    }

    [Fact]
    public async Task ResizeAsync_WithAspectRatio_ShouldFitWithinBox()
    {
        using var input = CreatePng(200, 100);
        var options = new ImageProcessingOptions
        {
            Width = 100,
            Height = 100,
            MaintainAspectRatio = true,
            OutputFormat = "png"
        };

        var bytes = await _sut.ResizeAsync(input, options);

        bytes.Should().NotBeEmpty();
        using var result = Image.Load(bytes);
        result.Width.Should().Be(100);
        result.Height.Should().Be(50);
    }

    [Fact]
    public async Task ResizeAsync_WithoutAspectRatio_ShouldForceExactDimensions()
    {
        using var input = CreatePng(200, 100);
        var options = new ImageProcessingOptions
        {
            Width = 50,
            Height = 50,
            MaintainAspectRatio = false,
            OutputFormat = "png"
        };

        var bytes = await _sut.ResizeAsync(input, options);

        using var result = Image.Load(bytes);
        result.Width.Should().Be(50);
        result.Height.Should().Be(50);
    }

    [Theory]
    [InlineData("png")]
    [InlineData("jpeg")]
    [InlineData("webp")]
    [InlineData("gif")]
    [InlineData("bmp")]
    public async Task ConvertFormatAsync_ShouldProduceLoadableBytes(string format)
    {
        using var input = CreatePng(40, 30);

        var bytes = await _sut.ConvertFormatAsync(input, format);

        bytes.Should().NotBeEmpty();
        using var loaded = Image.Load(bytes);
        loaded.Width.Should().Be(40);
        loaded.Height.Should().Be(30);
    }

    // v4 (ADR-0028 K6, D9a): an unrecognised format name throws instead of silently producing JPEG.
    [Theory]
    [InlineData("avif")]
    [InlineData("unknown-xyz")]
    public async Task ConvertFormatAsync_UnknownFormat_ThrowsAndListsSupportedFormats(string format)
    {
        using var input = CreatePng(20, 20);

        var act = () => _sut.ConvertFormatAsync(input, format);

        (await act.Should().ThrowAsync<NotSupportedException>())
            .WithMessage($"*{format}*")
            .And.Message.Should().ContainAll("jpeg", "png", "webp", "gif", "bmp");
    }

    [Theory]
    [InlineData("jpg")]
    [InlineData("jpeg")]
    [InlineData("JPEG")]
    public async Task ConvertFormatAsync_JpegAliases_ProduceJpeg(string format)
    {
        using var input = CreatePng(20, 20);

        var bytes = await _sut.ConvertFormatAsync(input, format);

        bytes.Take(2).Should().Equal(0xFF, 0xD8);
    }

    // v4 (ADR-0028 K6): an empty format keeps the source format, as the ImageProcessingOptions contract says.
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];

    [Fact]
    public async Task ResizeAsync_WithoutOutputFormat_KeepsSourceFormat()
    {
        using var input = CreatePng(20, 20);

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        bytes.Take(4).Should().Equal(PngSignature);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ConvertFormatAsync_EmptyFormat_KeepsSourceFormat(string format)
    {
        using var input = CreatePng(20, 20);

        var bytes = await _sut.ConvertFormatAsync(input, format);

        bytes.Take(4).Should().Equal(PngSignature);
    }

    [Fact]
    public async Task ResizeAsync_WithoutOutputFormat_UnsupportedSourceFormat_ProducesJpeg()
    {
        using var image = new Image<Rgba32>(20, 20);
        using var input = new MemoryStream();
        image.Save(input, new TiffEncoder());
        input.Position = 0;

        var bytes = await _sut.ResizeAsync(input, new ImageProcessingOptions { Width = 10, Height = 10 });

        bytes.Take(2).Should().Equal(0xFF, 0xD8);
    }

    [Fact]
    public async Task ResizeAsync_NullDimensions_ShouldPreserveOriginal()
    {
        using var input = CreatePng(64, 48);
        var options = new ImageProcessingOptions
        {
            Width = null,
            Height = null,
            OutputFormat = "png"
        };

        var bytes = await _sut.ResizeAsync(input, options);

        using var result = Image.Load(bytes);
        result.Width.Should().Be(64);
        result.Height.Should().Be(48);
    }
}
