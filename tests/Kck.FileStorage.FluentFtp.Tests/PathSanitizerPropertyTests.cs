using CsCheck;
using FluentAssertions;
using Kck.FileStorage.FluentFtp;
using Xunit;

namespace Kck.FileStorage.FluentFtp.Tests;

/// <summary>
/// Faz-B3: CsCheck property-based tests for PathSanitizer security invariants.
/// </summary>
public sealed class PathSanitizerPropertyTests
{
    private static readonly Gen<string> SafeSegmentGen =
        Gen.String[Gen.Char.AlphaNumeric, 1, 12];

    private static readonly Gen<string> SafePathGen =
        Gen.Select(SafeSegmentGen, SafeSegmentGen, SafeSegmentGen,
            (a, b, c) => $"{a}/{b}/{c}");

    [Fact]
    public void Validate_SafePaths_NeverThrow()
    {
        SafePathGen.Sample(path =>
        {
            var act = () => PathSanitizer.Validate(path);
            act.Should().NotThrow($"path '{path}' contains only safe alphanumeric segments");
        });
    }

    [Fact]
    public void Validate_PathsContainingDotDot_AlwaysThrow()
    {
        Gen.Select(SafeSegmentGen, SafeSegmentGen)
            .Sample((prefix, suffix) =>
            {
                var traversal = $"{prefix}/../{suffix}";
                var act = () => PathSanitizer.Validate(traversal);
                act.Should().Throw<ArgumentException>($"path '{traversal}' contains traversal");
            });
    }

    [Fact]
    public void Validate_AbsolutePaths_AlwaysThrow()
    {
        SafeSegmentGen.Sample(segment =>
        {
            var absolute = $"/{segment}/file.txt";
            var act = () => PathSanitizer.Validate(absolute);
            act.Should().Throw<ArgumentException>($"absolute path '{absolute}' must be rejected");
        });
    }

    [Fact]
    public void Validate_PathsWithPercentEncoding_AlwaysThrow()
    {
        Gen.Select(SafeSegmentGen, SafeSegmentGen)
            .Sample((prefix, suffix) =>
            {
                var encoded = $"{prefix}/%2e%2e/{suffix}";
                var act = () => PathSanitizer.Validate(encoded);
                act.Should().Throw<ArgumentException>($"percent-encoded path must be rejected");
            });
    }

    [Fact]
    public void Validate_PathsWithControlCharacters_AlwaysThrow()
    {
        Gen.Char['\x01', '\x1F'].Sample(controlChar =>
        {
            var path = $"safe/{controlChar}file.txt";
            var act = () => PathSanitizer.Validate(path);
            act.Should().Throw<ArgumentException>($"control char 0x{(int)controlChar:X2} must be rejected");
        });
    }
}
