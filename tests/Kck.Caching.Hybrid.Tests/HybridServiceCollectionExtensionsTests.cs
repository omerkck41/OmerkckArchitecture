using System.Diagnostics;
using AwesomeAssertions;
using Kck.Caching.Abstractions;
using Kck.Caching.Hybrid.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Kck.Caching.Hybrid.Tests;

/// <summary>
/// Registration follows the Kck.Caching.Redis pattern: the Redis configuration is checked at registration and the
/// connection is opened asynchronously by a hosted service at host start, never inside DI resolution.
/// </summary>
public sealed class HybridServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKckCachingHybrid_WithoutRedisConfiguration_ThrowsAtRegistration()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckCachingHybrid(_ => { });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Redis connection is not configured*");
    }

    [Fact]
    public async Task AddKckCachingHybrid_RegistersHostedServiceThatConnectsAtStart()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckCachingHybrid(redis => redis.Configuration = "localhost:1"); // nothing listens there
        await using var provider = services.BuildServiceProvider();

        var stopwatch = Stopwatch.StartNew();
        var hostedServices = provider.GetServices<IHostedService>().ToList();
        stopwatch.Stop();

        hostedServices.Should().ContainSingle(h => h is HybridRedisConnectionHostedService);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1), "resolving the hosted service must not connect");

        var act = () => provider.GetRequiredService<ICacheService>();
        act.Should().Throw<InvalidOperationException>().WithMessage("*has not been initialized*");
    }
}
