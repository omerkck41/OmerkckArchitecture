using System.Text;
using System.Text.Json;
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
    private readonly IDistributedCache _distributedCache = Substitute.For<IDistributedCache>();
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
        _sut = new HybridCacheService(_hybridCache, _distributedCache, _redis, options);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsDeserializedValue()
    {
        var expected = new TestDto("hello", 42);
        _distributedCache.GetAsync("test:mykey", Arg.Any<CancellationToken>())
            .Returns(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expected)));

        var result = await _sut.GetAsync<TestDto>("mykey");

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAsync_WhenKeyMissing_ReturnsNull()
    {
        _distributedCache.GetAsync("test:missing", Arg.Any<CancellationToken>())
            .Returns(default(byte[]?));

        var result = await _sut.GetAsync<TestDto>("missing");

        result.Should().BeNull();
    }

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

    private sealed record TestDto(string Name, int Value);
}
