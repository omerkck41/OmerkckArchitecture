using System.Diagnostics;
using AwesomeAssertions;
using Kck.EventBus.RabbitMq;
using Kck.EventBus.RabbitMq.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

public class RabbitMqHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_BusNotConnected_ReturnsUnhealthyWithoutConnecting()
    {
        var options = new RabbitMqOptions { HostName = "invalid-host-for-test", RetryCount = 1 };
        await using var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(options)
            .AddSingleton<RabbitMqEventBus>()
            .BuildServiceProvider();
        var check = ActivatorUtilities.CreateInstance<RabbitMqHealthCheck>(services);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("rabbitmq-eventbus", check, HealthStatus.Unhealthy, ["ready"])
        };
        var stopwatch = Stopwatch.StartNew();

        var result = await check.CheckHealthAsync(context);

        stopwatch.Stop();
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("not connected");
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }
}
