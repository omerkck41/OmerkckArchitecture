using AwesomeAssertions;
using Kck.Http.Abstractions;
using Kck.Http.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Http.Resilience.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    private const string OptionsName = "IApiClient-standard";

    [Fact]
    public void AddKckHttpResilience_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddKckHttpResilience();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddKckHttpResilience_ResolvesIApiClientAsResilientApiClient()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckHttpResilience();
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IApiClient>();

        client.Should().BeOfType<ResilientApiClient>();
    }

    [Fact]
    public void AddKckHttpResilience_WithConfigure_AppliesOptionsToStandardHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckHttpResilience(o =>
        {
            o.MaxRetryAttempts = 7;
            o.MedianFirstRetryDelay = TimeSpan.FromMilliseconds(250);
            o.CircuitBreakerDuration = TimeSpan.FromSeconds(45);
            o.Timeout = TimeSpan.FromSeconds(20);
        });
        using var provider = services.BuildServiceProvider();

        var opts = provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>().Get(OptionsName);

        opts.Retry.MaxRetryAttempts.Should().Be(7);
        opts.Retry.Delay.Should().Be(TimeSpan.FromMilliseconds(250));
        opts.CircuitBreaker.SamplingDuration.Should().Be(TimeSpan.FromSeconds(90));
        opts.AttemptTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(20));
        opts.TotalRequestTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void AddKckHttpResilience_WithoutConfigure_UsesResilienceOptionsDefaults()
    {
        var defaults = new ResilienceOptions();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckHttpResilience();
        using var provider = services.BuildServiceProvider();

        var opts = provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>().Get(OptionsName);

        opts.Retry.MaxRetryAttempts.Should().Be(defaults.MaxRetryAttempts);
        opts.Retry.Delay.Should().Be(defaults.MedianFirstRetryDelay);
        opts.AttemptTimeout.Timeout.Should().Be(defaults.Timeout);
        opts.TotalRequestTimeout.Timeout.Should().Be(defaults.Timeout * 3);
    }
}
