using AwesomeAssertions;
using Kck.FileStorage.Abstractions;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

public sealed class PathHelperTests
{
    [Theory]
    [InlineData("uploads/file.txt", true)]
    [InlineData("a/b/c.pdf", true)]
    [InlineData("singlefile.txt", true)]
    [InlineData("../etc/passwd", false)]
    [InlineData("subdir/../secret", false)]
    [InlineData("a/../../root", false)]
    [InlineData("~/.ssh/id_rsa", false)]
    [InlineData("home/~/config", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsValidPath_ReturnsExpected(string path, bool expected)
        => PathHelper.IsValidPath(path).Should().Be(expected);

    [Theory]
    [InlineData("a/../b", "a/b")]
    [InlineData("uploads/~/file.txt", "uploads/file.txt")]
    [InlineData("safe/path/file.txt", "safe/path/file.txt")]
    [InlineData("a\\..\\b", "a/b")]
    [InlineData("../secret", "secret")]
    public void Sanitize_RemovesDangerousSegments(string path, string expected)
        => PathHelper.Sanitize(path).Should().Be(expected);
}
