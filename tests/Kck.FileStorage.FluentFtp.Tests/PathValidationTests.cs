using AwesomeAssertions;
using Kck.FileStorage.Abstractions;
using Xunit;

namespace Kck.FileStorage.FluentFtp.Tests;

// The FTP provider's path rule (formerly its internal PathSanitizer) is PathHelper.Validate, shared with Blob and S3.
public class PathValidationTests
{
    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    [InlineData("/absolute/path")]
    [InlineData("normal/../../../escape")]
    public void Validate_WithTraversalAttempt_ShouldThrow(string maliciousPath)
    {
        var act = () => PathHelper.Validate(maliciousPath);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("uploads/images/photo.jpg")]
    [InlineData("documents/report.pdf")]
    [InlineData("data/exports/2026/file.csv")]
    public void Validate_WithSafePath_ShouldNotThrow(string safePath)
    {
        var act = () => PathHelper.Validate(safePath);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithNullOrWhitespace_ShouldThrow(string? path)
    {
        var act = () => PathHelper.Validate(path!);

        act.Should().Throw<ArgumentException>();
    }
}
