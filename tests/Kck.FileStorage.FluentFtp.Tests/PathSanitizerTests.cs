using FluentAssertions;
using Kck.FileStorage.FluentFtp;
using Xunit;

namespace Kck.FileStorage.FluentFtp.Tests;

public class PathSanitizerTests
{
    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    [InlineData("/absolute/path")]
    [InlineData("normal/../../../escape")]
    public void Validate_WithTraversalAttempt_ShouldThrow(string maliciousPath)
    {
        var act = () => PathSanitizer.Validate(maliciousPath);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("uploads/images/photo.jpg")]
    [InlineData("documents/report.pdf")]
    [InlineData("data/exports/2026/file.csv")]
    public void Validate_WithSafePath_ShouldNotThrow(string safePath)
    {
        var act = () => PathSanitizer.Validate(safePath);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithNullOrWhitespace_ShouldThrow(string? path)
    {
        var act = () => PathSanitizer.Validate(path!);

        act.Should().Throw<ArgumentException>();
    }
}
