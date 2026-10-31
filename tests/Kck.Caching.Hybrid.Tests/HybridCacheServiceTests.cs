using AwesomeAssertions;
using Kck.Caching.Abstractions;
using Kck.Caching.Hybrid;
using Kck.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Kck.Caching.Hybrid.Tests;

public sealed class HybridCacheServiceTests
{
    private readonly HybridCache _hybridCache = Substitute.For<HybridCache>();
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly HybridCacheService _sut;

    public HybridCacheServiceTests()
    {
        var options = new StaticOptionsMonitor<HybridCacheOptions>(new HybridCacheOptions
        {
            KeyPrefix = "test:",
            DefaultExpiration = TimeSpan.FromMinutes(5),
            DefaultLocalExpiration = TimeSpan.FromMinutes(1)
        });
        _sut = new HybridCacheService(_hybridCache, _redis, options);
    }

    // GetAsync is covered with the real HybridCache in HybridCacheRoundTripTests (a mock cannot reproduce its L2 format).

    [Fact]
    public async Task SetAsync_CallsHybridCacheSet()
    {
        var dto = new TestDto("v", 1);

        await _sut.SetAsync("k", dto, TimeSpan.FromMinutes(3));

        await _hybridCache.Received(1).SetAsync(
            "test:k",
            dto,
            Arg.Is<HybridCacheEntryOptions>(o => o.Expiration == TimeSpan.FromMinutes(3)),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_CallsHybridCacheRemove()
    {
        await _sut.RemoveAsync("gone");

        await _hybridCache.Received(1).RemoveAsync("test:gone", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ReturnsTrue()
    {
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.KeyExistsAsync(new RedisKey("test:exists"), Arg.Any<CommandFlags>()).Returns(true);

        var result = await _sut.ExistsAsync("exists");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyMissing_ReturnsFalse()
    {
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.KeyExistsAsync(new RedisKey("test:nope"), Arg.Any<CommandFlags>()).Returns(false);

        var result = await _sut.ExistsAsync("nope");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetAsync_WithNullExpiration_UsesDefaultExpiration()
    {
        var dto = new TestDto("v", 0);

        await _sut.SetAsync("defexp", dto);

        await _hybridCache.Received(1).SetAsync(
            "test:defexp",
            dto,
            Arg.Is<HybridCacheEntryOptions>(o => o.Expiration == TimeSpan.FromMinutes(5)),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<CancellationToken>());
    }

    // The L2 RedisCache stores every key as InstanceName + key; HybridCache's own L1 keys have no InstanceName.
    private HybridCacheService WithInstanceName(string instanceName) => new(
        _hybridCache, _redis,
        new StaticOptionsMonitor<HybridCacheOptions>(new HybridCacheOptions { KeyPrefix = "test:" }),
        Microsoft.Extensions.Options.Options.Create(
            new Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions { InstanceName = instanceName }));

    [Fact]
    public async Task ExistsAsync_WithInstanceName_ChecksTheStoredKey()
    {
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.KeyExistsAsync(new RedisKey("app:test:exists"), Arg.Any<CommandFlags>()).Returns(true);

        var result = await WithInstanceName("app:").ExistsAsync("exists");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WithInstanceName_ScansStoredKeysAndEvictsL1KeysWithoutIt()
    {
        var server = Substitute.For<IServer>();
        var db = Substitute.For<IDatabase>();
        _redis.GetEndPoints(Arg.Any<bool>()).Returns([new System.Net.DnsEndPoint("localhost", 6379)]);
        _redis.GetServer(Arg.Any<System.Net.EndPoint>(), Arg.Any<object?>()).Returns(server);
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        server.KeysAsync(Arg.Any<int>(), new RedisValue("app:test:orders:*"), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<int>(),
                Arg.Any<CommandFlags>())
            .Returns(new[] { new RedisKey("app:test:orders:1") }.ToAsyncEnumerable());

        await WithInstanceName("app:").RemoveByPrefixAsync("orders:");

        await db.Received(1).KeyDeleteAsync(Arg.Is<RedisKey[]>(k => k.Single() == "app:test:orders:1"), Arg.Any<CommandFlags>());
        await _hybridCache.Received(1).RemoveAsync(
            Arg.Is<IEnumerable<string>>(k => k.Single() == "test:orders:1"), Arg.Any<CancellationToken>());
    }

    private sealed record TestDto(string Name, int Value);
}
