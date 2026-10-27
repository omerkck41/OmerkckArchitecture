using AwesomeAssertions;
using Kck.Caching.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Redis;
using Xunit;

namespace Kck.Caching.Hybrid.Tests;

/// <summary>
/// Real Redis container tests for the L2 path: Microsoft HybridCache over
/// Microsoft.Extensions.Caching.StackExchangeRedis and StackExchange.Redis.
/// Catches binary incompatibilities between those packages that mocks cannot.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001
public sealed class HybridCacheIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine").Build();

    public Task InitializeAsync() => _redis.StartAsync();

    public async Task DisposeAsync() => await _redis.DisposeAsync();

    private ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckCachingHybrid(redis => redis.Configuration = _redis.GetConnectionString());
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SetAsync_ValueIsReadFromRedisByAnotherInstance()
    {
        await using (var writer = CreateProvider())
        {
            await writer.GetRequiredService<ICacheService>().SetAsync("order:42", "shipped");
        }

        // A fresh provider has an empty L1, so the value can only come from Redis (L2).
        await using var reader = CreateProvider();
        var cache = reader.GetRequiredService<ICacheService>();

        (await cache.GetAsync<string>("order:42")).Should().Be("shipped");
        (await cache.ExistsAsync("order:42")).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveByPrefixAsync_DeletesMatchingKeysInRedis()
    {
        await using var provider = CreateProvider();
        var cache = provider.GetRequiredService<ICacheService>();
        await cache.SetAsync("tenant:a:1", "x");
        await cache.SetAsync("tenant:a:2", "y");
        await cache.SetAsync("tenant:b:1", "z");

        await cache.RemoveByPrefixAsync("tenant:a:");

        (await cache.ExistsAsync("tenant:a:1")).Should().BeFalse();
        (await cache.ExistsAsync("tenant:a:2")).Should().BeFalse();
        (await cache.ExistsAsync("tenant:b:1")).Should().BeTrue();
    }
}
