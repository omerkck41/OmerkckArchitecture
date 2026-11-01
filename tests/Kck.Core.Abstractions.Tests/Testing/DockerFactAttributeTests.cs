using AwesomeAssertions;
using Kck.Testing;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Testing;

/// <summary>
/// The shared <see cref="DockerFactAttribute"/>/<see cref="DockerTheoryAttribute"/> (tests/_Shared) skip container
/// tests when no Docker daemon is reachable, except on CI or when KCK_REQUIRE_DOCKER=1 asks for a loud failure.
/// The probe is called with injected environment values so the process-wide cached result is never touched.
/// </summary>
public sealed class DockerFactAttributeTests
{
    private static string? NoVariables(string name) => null;

    [Fact]
    public void DockerFact_WhenDaemonUnreachable_SetsSkip()
    {
        // tcp://127.0.0.1:1 — nothing listens on port 1, so the probe reports the daemon as unreachable.
        Func<bool> unreachableProbe = () => DockerProbe.IsAvailable(() => "tcp://127.0.0.1:1");

        var fact = new DockerFactAttribute(NoVariables, unreachableProbe);
        var theory = new DockerTheoryAttribute(NoVariables, unreachableProbe);

        fact.Skip.Should().Be(DockerProbe.SkipReason);
        theory.Skip.Should().Be(DockerProbe.SkipReason);
        DockerProbe.SkipReason.Should().Contain("KCK_REQUIRE_DOCKER=1");
    }

    [Fact]
    public void DockerFact_WhenCiVariableSet_NeverSkips()
    {
        static string? Ci(string name) => name == "CI" ? "true" : null;
        static string? RequireDocker(string name) => name == "KCK_REQUIRE_DOCKER" ? "1" : null;
        Func<bool> unreachable = () => false;

        new DockerFactAttribute(Ci, unreachable).Skip.Should().BeNull();
        new DockerTheoryAttribute(Ci, unreachable).Skip.Should().BeNull();
        new DockerFactAttribute(RequireDocker, unreachable).Skip.Should().BeNull();
        new DockerTheoryAttribute(RequireDocker, unreachable).Skip.Should().BeNull();
    }

    [Theory]
    [InlineData("npipe://")]
    [InlineData("garbage")]
    public void DockerProbe_MalformedDockerHost_ReportsUnreachable(string dockerHost)
    {
        // The probe runs inside a Lazy during test discovery: it must never throw.
        var probe = () => DockerProbe.IsAvailable(() => dockerHost);

        probe.Should().NotThrow().Which.Should().BeFalse();
    }

    [Fact]
    public void DockerProbe_NoUnixSocketCandidateExists_ReportsUnreachable()
    {
        var missing = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "docker.sock");

        DockerProbe.AnyUnixSocketReachable([missing, missing + ".2"]).Should().BeFalse();
    }
}
