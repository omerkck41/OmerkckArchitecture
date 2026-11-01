using AwesomeAssertions;
using Kck.FileStorage.Abstractions;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

public sealed class PathHelperTests
{
    // K7 (2026-10-31 health analysis, probe P6): the shared check the Blob, S3 and FTP providers run on every path.
    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    [InlineData("normal/../../../escape")]
    [InlineData("/absolute/path")]
    [InlineData("\\absolute\\path")]
    [InlineData("a/%2e%2e/b")]
    [InlineData("safe/\u0001file.txt")]
    [InlineData("nul\0byte.txt")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsTraversalAbsoluteEncodedAndControlCharacterPaths(string path)
    {
        var act = () => PathHelper.Validate(path);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("uploads/images/photo.jpg")]
    [InlineData("documents/report.pdf")]
    [InlineData("file.txt")]
    [InlineData("data\\exports\\2026\\file.csv")]
    public void Validate_AcceptsRelativePaths(string path)
    {
        var act = () => PathHelper.Validate(path);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_NullPath_Throws()
    {
        var act = () => PathHelper.Validate(null!);

        act.Should().Throw<ArgumentException>();
    }
}
