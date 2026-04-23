using FluentAssertions;
using Kck.AspNetCore.Sanitization;
using Xunit;

namespace Kck.AspNetCore.Tests;

public class InputSanitizerTests
{
    private readonly InputSanitizer _sut = new();

    [Theory]
    [InlineData("<script>alert('xss')</script>", "scriptalertxss/script")]
    [InlineData("hello<world>", "helloworld")]
    [InlineData("a&b\"cd", "abcd")]
    public void Sanitize_WithDangerousChars_ShouldRemoveThem(string input, string expected)
    {
        var result = _sut.Sanitize(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Hello World")]
    [InlineData("normal text 123")]
    public void Sanitize_WithSafeInput_ShouldReturnUnchanged(string input)
    {
        var result = _sut.Sanitize(input);
        result.Should().Be(input);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Sanitize_WithNullOrWhitespace_ShouldThrow(string? input)
    {
        var act = () => _sut.Sanitize(input!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("abc123", true)]
    [InlineData("ABC", true)]
    [InlineData("abc 123", false)]
    [InlineData("abc!@#", false)]
    [InlineData("", true)]
    public void IsAlphanumeric_ShouldReturnExpected(string input, bool expected)
    {
        var result = _sut.IsAlphanumeric(input);
        result.Should().Be(expected);
    }
}
