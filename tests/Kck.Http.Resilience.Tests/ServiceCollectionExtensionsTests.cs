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
            o.CircuitBreakerFailureRatio = 0.5;
            o.CircuitBreakerMinimumThroughput = 20;
            o.Timeout = TimeSpan.FromSeconds(20);
        });
        using var provider = services.BuildServiceProvider();

        var opts = provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>().Get(OptionsName);

        opts.Retry.MaxRetryAttempts.Should().Be(7);
        opts.Retry.Delay.Should().Be(TimeSpan.FromMilliseconds(250));
        // CircuitBreakerDuration is how long the circuit stays open (before 4.7 it set the sampling window).
        opts.CircuitBreaker.BreakDuration.Should().Be(TimeSpan.FromSeconds(45));
        opts.CircuitBreaker.FailureRatio.Should().Be(0.5);
        opts.CircuitBreaker.MinimumThroughput.Should().Be(20);
        opts.CircuitBreaker.SamplingDuration.Should().Be(TimeSpan.FromSeconds(40), "twice the attempt timeout");
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
        opts.CircuitBreaker.BreakDuration.Should().Be(defaults.CircuitBreakerDuration);
        opts.CircuitBreaker.FailureRatio.Should().Be(defaults.CircuitBreakerFailureRatio);
        opts.CircuitBreaker.MinimumThroughput.Should().Be(defaults.CircuitBreakerMinimumThroughput);
    }

    [Fact]
    public void AddKckHttpResilience_ShortBreakWithLongTimeout_PassesStandardHandlerValidation()
    {
        // The standard handler requires a sampling window of at least twice the attempt timeout; it is derived from
        // Timeout now, so a short break duration no longer makes the configuration invalid.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckHttpResilience(o =>
        {
            o.CircuitBreakerDuration = TimeSpan.FromSeconds(5);
            o.Timeout = TimeSpan.FromSeconds(40);
        });
        using var provider = services.BuildServiceProvider();

        var resolve = () => provider.GetRequiredService<IApiClient>();

        resolve.Should().NotThrow();
        provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>().Get(OptionsName)
            .CircuitBreaker.SamplingDuration.Should().Be(TimeSpan.FromSeconds(80));
    }

    [Fact]
    public void AddKckHttpResilience_ShortTimeout_KeepsAThirtySecondSamplingFloor()
    {
        // 2 x Timeout alone would give a window below the handler's 0.5 s minimum for a 100 ms timeout.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckHttpResilience(o => o.Timeout = TimeSpan.FromMilliseconds(100));
        using var provider = services.BuildServiceProvider();

        var resolve = () => provider.GetRequiredService<IApiClient>();

        resolve.Should().NotThrow();
        provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>().Get(OptionsName)
            .CircuitBreaker.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddKckHttpResilience_InvalidCircuitBreakerValue_FailsStartupValidation()
    {
        // Invalid values used to surface only when the first IApiClient was created.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckHttpResilience(o => o.CircuitBreakerFailureRatio = 1.5);
        using var provider = services.BuildServiceProvider();

        var validateAtStartup = () => provider.GetRequiredService<IStartupValidator>().Validate();

        validateAtStartup.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void ResilienceOptions_CircuitBreakerDefaults_MatchTheStandardHandler()
    {
        var defaults = new ResilienceOptions();
        var standard = new HttpStandardResilienceOptions();

        defaults.CircuitBreakerFailureRatio.Should().Be(standard.CircuitBreaker.FailureRatio);
        defaults.CircuitBreakerMinimumThroughput.Should().Be(standard.CircuitBreaker.MinimumThroughput);
    }
}
