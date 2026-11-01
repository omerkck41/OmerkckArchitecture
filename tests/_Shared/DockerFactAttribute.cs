using Xunit;

namespace Kck.Testing;

/// <summary>
/// A <see cref="FactAttribute"/> for tests that need a Docker daemon (Testcontainers). Skipped with a clear reason
/// when no daemon is reachable; never skipped when <c>CI=true</c> or <c>KCK_REQUIRE_DOCKER=1</c>.
/// See <see cref="DockerProbe"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        Skip = DockerProbe.CurrentSkipReason();
    }

    /// <summary>Test seam: evaluates the skip rule with injected environment and probe.</summary>
    internal DockerFactAttribute(Func<string, string?> getEnvironmentVariable, Func<bool> isDockerAvailable)
    {
        Skip = DockerProbe.GetSkipReason(getEnvironmentVariable, isDockerAvailable);
    }
}

/// <summary>
/// A <see cref="TheoryAttribute"/> for tests that need a Docker daemon (Testcontainers); same rule as
/// <see cref="DockerFactAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class DockerTheoryAttribute : TheoryAttribute
{
    public DockerTheoryAttribute()
    {
        Skip = DockerProbe.CurrentSkipReason();
    }

    /// <summary>Test seam: evaluates the skip rule with injected environment and probe.</summary>
    internal DockerTheoryAttribute(Func<string, string?> getEnvironmentVariable, Func<bool> isDockerAvailable)
    {
        Skip = DockerProbe.GetSkipReason(getEnvironmentVariable, isDockerAvailable);
    }
}
