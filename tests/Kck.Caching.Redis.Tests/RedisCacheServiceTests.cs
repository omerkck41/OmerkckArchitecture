using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kck.Caching.Abstractions;
using Kck.Caching.Redis;
using Kck.Testing;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Kck.Caching.Redis.Tests;

public sealed class RedisCacheServiceTests
{
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly RedisCacheService _sut;

    public RedisCacheServiceTests()
    {
        var options = new StaticOptionsMonitor<CacheOptions>(new CacheOptions { KeyPrefix = "test:" });
        _sut = new RedisCacheService(_cache, _redis, options);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsDeserializedValue()
    {
        var expected = new TestDto("hello", 42);
        var json = JsonSerializer.Serialize(expected);
        var bytes = Encoding.UTF8.GetBytes(json);
        _cache.GetAsync("test:mykey", Arg.Any<CancellationToken>())
            .Returns(bytes);

        var result = await _sut.GetAsync<TestDto>("mykey");

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAsync_WhenKeyMissing_ReturnsNull()
    {
        _cache.GetAsync("test:missing", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        var result = await _sut.GetAsync<TestDto>("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_CallsDistributedCacheWithCorrectKey()
    {
        var dto = new TestDto("value", 1);

        await _sut.SetAsync("setkey", dto, TimeSpan.FromMinutes(5));

        await _cache.Received(1).SetAsync(
            "test:setkey",
            Arg.Any<byte[]>(),
            Arg.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_CallsDistributedCacheRemove()
    {
        await _sut.RemoveAsync("delkey");

        await _cache.Received(1).RemoveAsync("test:delkey", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ReturnsTrue()
    {
        _cache.GetAsync("test:exists", Arg.Any<CancellationToken>())
            .Returns(Encoding.UTF8.GetBytes("{}"));

        var result = await _sut.ExistsAsync("exists");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyMissing_ReturnsFalse()
    {
        _cache.GetAsync("test:nope", Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        var result = await _sut.ExistsAsync("nope");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetAsync_WithNullExpiration_UsesDefaultExpiration()
    {
        var dto = new TestDto("v", 0);

        await _sut.SetAsync("defexp", dto);

        await _cache.Received(1).SetAsync(
            "test:defexp",
            Arg.Any<byte[]>(),
            Arg.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
            Arg.Any<CancellationToken>());
    }

    private sealed record TestDto(string Name, int Value);
}
