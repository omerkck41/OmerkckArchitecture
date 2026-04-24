using FluentAssertions;
using Kck.Documents.Abstractions;
using Kck.Documents.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
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

    [Fact]
    public async Task ConvertFormatAsync_UnknownFormat_ShouldFallbackToJpeg()
    {
        using var input = CreatePng(20, 20);

        var bytes = await _sut.ConvertFormatAsync(input, "unknown-xyz");

        bytes.Should().NotBeEmpty();
        using var loaded = Image.Load(bytes);
        loaded.Width.Should().Be(20);
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
