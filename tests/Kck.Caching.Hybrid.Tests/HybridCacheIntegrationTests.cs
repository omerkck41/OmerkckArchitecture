using AwesomeAssertions;
using Kck.Caching.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

    private async Task<ServiceProvider> CreateProviderAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckCachingHybrid(redis => redis.Configuration = _redis.GetConnectionString());
        var provider = services.BuildServiceProvider();
        await StartAsync(provider);
        return provider;
    }

    // What the host does at startup: the Redis connection is opened by a hosted service.
    private static async Task StartAsync(ServiceProvider provider)
    {
        foreach (var hostedService in provider.GetServices<IHostedService>())
            await hostedService.StartAsync(CancellationToken.None);
    }

    [Fact]
    public async Task SetAsync_ValueIsReadFromRedisByAnotherInstance()
    {
        await using (var writer = await CreateProviderAsync())
        {
            await writer.GetRequiredService<ICacheService>().SetAsync("order:42", "shipped");
        }

        // A fresh provider has an empty L1, so the value can only come from Redis (L2).
        await using var reader = await CreateProviderAsync();
        var cache = reader.GetRequiredService<ICacheService>();

        (await cache.GetAsync<string>("order:42")).Should().Be("shipped");
        (await cache.ExistsAsync("order:42")).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveByPrefixAsync_DeletesMatchingKeysInRedis()
    {
        await using var provider = await CreateProviderAsync();
        var cache = provider.GetRequiredService<ICacheService>();
        await cache.SetAsync("tenant:a:1", "x");
        await cache.SetAsync("tenant:a:2", "y");
        await cache.SetAsync("tenant:b:1", "z");

        await cache.RemoveByPrefixAsync("tenant:a:");

        (await cache.ExistsAsync("tenant:a:1")).Should().BeFalse();
        (await cache.ExistsAsync("tenant:a:2")).Should().BeFalse();
        (await cache.ExistsAsync("tenant:b:1")).Should().BeTrue();
    }

    [Fact]
    public async Task InstanceName_ExistsAndRemoveByPrefix_FindTheStoredKeys()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckCachingHybrid(redis =>
        {
            redis.Configuration = _redis.GetConnectionString();
            redis.InstanceName = "app:";
        });
        await using var provider = services.BuildServiceProvider();
        await StartAsync(provider);
        var cache = provider.GetRequiredService<ICacheService>();
        await cache.SetAsync("inst:a", "1");
        await cache.SetAsync("other:b", "2");

        (await cache.ExistsAsync("inst:a")).Should().BeTrue();
        await cache.RemoveByPrefixAsync("inst");

        (await cache.ExistsAsync("inst:a")).Should().BeFalse();
        (await cache.GetAsync<string>("inst:a")).Should().BeNull(); // L1 evicted too
        (await cache.ExistsAsync("other:b")).Should().BeTrue();
    }
}
