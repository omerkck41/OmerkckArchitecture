using FluentAssertions;
using Xunit;

namespace Kck.Localization.Tests;

public sealed class DefaultPluralizerTests
{
    private readonly DefaultPluralizer _sut = new();

    [Theory]
    [InlineData(0, "other")]
    [InlineData(1, "one")]
    [InlineData(2, "other")]
    [InlineData(5, "other")]
    [InlineData(100, "other")]
    public void GetPluralCategory_English_ReturnsCorrectCategory(int count, string expected)
    {
        _sut.GetPluralCategory(count, "en").Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "other")]
    [InlineData(1, "one")]
    [InlineData(2, "other")]
    public void GetPluralCategory_Turkish_ReturnsCorrectCategory(int count, string expected)
    {
        _sut.GetPluralCategory(count, "tr").Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "zero")]
    [InlineData(1, "one")]
    [InlineData(2, "two")]
    [InlineData(3, "few")]
    [InlineData(10, "few")]
    [InlineData(11, "many")]
    [InlineData(99, "many")]
    [InlineData(100, "other")]
    [InlineData(200, "other")]
    public void GetPluralCategory_Arabic_ReturnsCorrectCategory(int count, string expected)
    {
        _sut.GetPluralCategory(count, "ar").Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "many")]
    [InlineData(1, "one")]
    [InlineData(2, "few")]
    [InlineData(4, "few")]
    [InlineData(5, "many")]
    [InlineData(12, "many")]
    [InlineData(22, "few")]
    [InlineData(25, "many")]
    public void GetPluralCategory_Polish_ReturnsCorrectCategory(int count, string expected)
    {
        _sut.GetPluralCategory(count, "pl").Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "one")]
    [InlineData(1, "one")]
    [InlineData(2, "other")]
    [InlineData(1000000, "many")]
    public void GetPluralCategory_French_ReturnsCorrectCategory(int count, string expected)
    {
        _sut.GetPluralCategory(count, "fr").Should().Be(expected);
    }

    [Fact]
    public void GetPluralCategory_UnknownCulture_FallsBackToEnglishRules()
    {
        _sut.GetPluralCategory(1, "xx").Should().Be("one");
        _sut.GetPluralCategory(5, "xx").Should().Be("other");
    }

    [Fact]
    public void GetPluralCategory_CultureWithRegion_UsesBaseLanguage()
    {
        _sut.GetPluralCategory(1, "en-US").Should().Be("one");
        _sut.GetPluralCategory(0, "ar-SA").Should().Be("zero");
    }
}
