using AwesomeAssertions;
using Kck.Caching.Abstractions;
using Kck.Caching.Redis;
using Kck.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace Kck.Caching.Redis.Tests;

/// <summary>
/// LS-FAZ-4.5: Real Redis container integration tests.
/// Verifies behaviours that mocks cannot catch: key expiry, SCAN-based prefix
/// deletion, and the EXISTS-only hot path.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001
public sealed class RedisCacheIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private RedisCacheService _sut = default!;
    private ConnectionMultiplexer _muxer = default!;

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();

        var cs = _redis.GetConnectionString();
        _muxer = await ConnectionMultiplexer.ConnectAsync(cs);

        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(opts => opts.Configuration = cs);
        var cache = services.BuildServiceProvider().GetRequiredService<IDistributedCache>();

        var options = new StaticOptionsMonitor<CacheOptions>(new CacheOptions
        {
            KeyPrefix = "test:",
            DefaultExpiration = TimeSpan.FromMinutes(5)
        });

        _sut = new RedisCacheService(cache, _muxer, options);
    }

    public async Task DisposeAsync()
    {
        await _muxer.CloseAsync();
        await _redis.DisposeAsync();
    }

    [DockerFact]
    public async Task SetAsync_GetAsync_RoundTrip()
    {
        var item = new TestCacheItem("integration", 7);

        await _sut.SetAsync("round-trip", item, TimeSpan.FromMinutes(1));
        var result = await _sut.GetAsync<TestCacheItem>("round-trip");

        result.Should().NotBeNull();
        result!.Name.Should().Be("integration");
        result.Value.Should().Be(7);
    }

    [DockerFact]
    public async Task ExistsAsync_AfterSet_ReturnsTrue()
    {
        await _sut.SetAsync("exists-key", "payload");

        (await _sut.ExistsAsync("exists-key")).Should().BeTrue();
    }

    [DockerFact]
    public async Task RemoveAsync_AfterSet_KeyNoLongerExists()
    {
        await _sut.SetAsync("remove-key", "payload");
        await _sut.RemoveAsync("remove-key");

        (await _sut.ExistsAsync("remove-key")).Should().BeFalse();
    }

    [DockerFact]
    public async Task RemoveByPrefixAsync_DeletesMatchingKeysOnly()
    {
        await _sut.SetAsync("grp:a", "1");
        await _sut.SetAsync("grp:b", "2");
        await _sut.SetAsync("other:c", "3");

        await _sut.RemoveByPrefixAsync("grp");

        (await _sut.ExistsAsync("grp:a")).Should().BeFalse();
        (await _sut.ExistsAsync("grp:b")).Should().BeFalse();
        (await _sut.ExistsAsync("other:c")).Should().BeTrue();
    }

    [DockerFact]
    public async Task InstanceName_ExistsAndRemoveByPrefix_FindTheStoredKeys()
    {
        // Built by the container, as AddKckCachingRedis does, with an InstanceName set.
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(o =>
        {
            o.Configuration = _redis.GetConnectionString();
            o.InstanceName = "app:";
        });
        services.AddSingleton<IConnectionMultiplexer>(_muxer);
        services.AddSingleton<Microsoft.Extensions.Options.IOptionsMonitor<CacheOptions>>(
            new StaticOptionsMonitor<CacheOptions>(new CacheOptions { KeyPrefix = "test:", DefaultExpiration = TimeSpan.FromMinutes(5) }));
        services.AddSingleton<RedisCacheService>();
        await using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<RedisCacheService>();

        await sut.SetAsync("inst:a", "1");
        await sut.SetAsync("other:b", "2");

        (await sut.ExistsAsync("inst:a")).Should().BeTrue();
        (await _muxer.GetDatabase().KeyExistsAsync("app:test:inst:a")).Should().BeTrue();
        await sut.RemoveByPrefixAsync("inst");
        (await sut.ExistsAsync("inst:a")).Should().BeFalse();
        (await sut.GetAsync<string>("inst:a")).Should().BeNull();
        (await sut.ExistsAsync("other:b")).Should().BeTrue();
    }

    [DockerFact]
    public async Task GetOrSetAsync_ValueTypes_MissRunsTheFactoryOnce_ThenHits()
    {
        // K2 (2026-10-31 health analysis, probe P1): int/bool/Guid misses returned 0/false/empty without the factory.
        var intCalls = 0;
        var guid = Guid.NewGuid();

        var first = await _sut.GetOrSetAsync("vt:int", () => { intCalls++; return Task.FromResult(42); });
        var second = await _sut.GetOrSetAsync("vt:int", () => { intCalls++; return Task.FromResult(-1); });

        first.Should().Be(42);
        second.Should().Be(42);
        intCalls.Should().Be(1);
        (await _sut.GetOrSetAsync("vt:bool", () => Task.FromResult(true))).Should().BeTrue();
        (await _sut.GetOrSetAsync("vt:guid", () => Task.FromResult(guid))).Should().Be(guid);
    }

    private sealed record TestCacheItem(string Name, int Value);
}
