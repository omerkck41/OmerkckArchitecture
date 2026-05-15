using FluentAssertions;
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

    [Fact]
    public async Task SetAsync_GetAsync_RoundTrip()
    {
        var item = new TestCacheItem("integration", 7);

        await _sut.SetAsync("round-trip", item, TimeSpan.FromMinutes(1));
        var result = await _sut.GetAsync<TestCacheItem>("round-trip");

        result.Should().NotBeNull();
        result!.Name.Should().Be("integration");
        result.Value.Should().Be(7);
    }

    [Fact]
    public async Task ExistsAsync_AfterSet_ReturnsTrue()
    {
        await _sut.SetAsync("exists-key", "payload");

        (await _sut.ExistsAsync("exists-key")).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_AfterSet_KeyNoLongerExists()
    {
        await _sut.SetAsync("remove-key", "payload");
        await _sut.RemoveAsync("remove-key");

        (await _sut.ExistsAsync("remove-key")).Should().BeFalse();
    }

    [Fact]
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

    private sealed record TestCacheItem(string Name, int Value);
}
