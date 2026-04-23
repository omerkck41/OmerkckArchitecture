using FluentAssertions;
using Kck.Observability.Abstractions;
using Kck.Observability.OpenTelemetry.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;
using KckHealthCheckResult = Kck.Observability.Abstractions.HealthCheckResult;
using KckHealthStatus = Kck.Observability.Abstractions.HealthStatus;

namespace Kck.Observability.OpenTelemetry.Tests;

public sealed class KckHealthCheckAdapterTests
{
    [Fact]
    public async Task CheckHealthAsync_Healthy_ReturnsHealthy()
    {
        var adapter = new KckHealthCheckAdapter(_ =>
            Task.FromResult(new KckHealthCheckResult(KckHealthStatus.Healthy, "ok")));

        var result = await adapter.CheckHealthAsync(CreateContext());

        result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy);
        result.Description.Should().Be("ok");
    }

    [Fact]
    public async Task CheckHealthAsync_Degraded_ReturnsDegraded()
    {
        var adapter = new KckHealthCheckAdapter(_ =>
            Task.FromResult(new KckHealthCheckResult(KckHealthStatus.Degraded, "slow")));

        var result = await adapter.CheckHealthAsync(CreateContext());

        result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);
    }

    [Fact]
    public async Task CheckHealthAsync_Unhealthy_ReturnsUnhealthy()
    {
        var adapter = new KckHealthCheckAdapter(_ =>
            Task.FromResult(new KckHealthCheckResult(KckHealthStatus.Unhealthy, "down")));

        var result = await adapter.CheckHealthAsync(CreateContext());

        result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
    }

    private static HealthCheckContext CreateContext() =>
        new() { Registration = new HealthCheckRegistration("test", _ => null!, null, null) };
}
