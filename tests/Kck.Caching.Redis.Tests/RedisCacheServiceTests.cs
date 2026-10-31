using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Kck.Caching.Abstractions;
using Kck.Caching.Redis;
using Kck.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
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
            .Returns(default(byte[]?));

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
        // LS-FAZ-5 (5.6): ExistsAsync artik IConnectionMultiplexer.GetDatabase().KeyExistsAsync
        // kullaniyor — IDistributedCache.GetAsync degil.
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.KeyExistsAsync(new RedisKey("test:exists"), Arg.Any<CommandFlags>())
            .Returns(true);

        var result = await _sut.ExistsAsync("exists");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyMissing_ReturnsFalse()
    {
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.KeyExistsAsync(new RedisKey("test:nope"), Arg.Any<CommandFlags>())
            .Returns(false);

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

    // RedisCache stores every key as InstanceName + key; the direct Redis commands must use the same name.
    private RedisCacheService WithInstanceName(string instanceName) => new(
        _cache, _redis, new StaticOptionsMonitor<CacheOptions>(new CacheOptions { KeyPrefix = "test:" }),
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
    public async Task RemoveByPrefixAsync_WithInstanceName_ScansTheStoredKeys()
    {
        var server = Substitute.For<IServer>();
        _redis.GetEndPoints(Arg.Any<bool>()).Returns([new System.Net.DnsEndPoint("localhost", 6379)]);
        _redis.GetServer(Arg.Any<System.Net.EndPoint>(), Arg.Any<object?>()).Returns(server);
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(Substitute.For<IDatabase>());
        server.KeysAsync(Arg.Any<int>(), Arg.Any<RedisValue>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<CommandFlags>())
            .Returns(AsyncEnumerable.Empty<RedisKey>());

        await WithInstanceName("app:").RemoveByPrefixAsync("orders:");

        server.Received(1).KeysAsync(Arg.Any<int>(), new RedisValue("app:test:orders:*"), Arg.Any<int>(), Arg.Any<long>(),
            Arg.Any<int>(), Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task RemoveByPrefixAsync_GlobCharactersInTheName_AreEscaped()
    {
        // Unescaped, "svc?" would also match "svcA..." keys of another instance on the same Redis.
        var server = Substitute.For<IServer>();
        _redis.GetEndPoints(Arg.Any<bool>()).Returns([new System.Net.DnsEndPoint("localhost", 6379)]);
        _redis.GetServer(Arg.Any<System.Net.EndPoint>(), Arg.Any<object?>()).Returns(server);
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(Substitute.For<IDatabase>());
        server.KeysAsync(Arg.Any<int>(), Arg.Any<RedisValue>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<CommandFlags>())
            .Returns(AsyncEnumerable.Empty<RedisKey>());

        await WithInstanceName("svc?[1]:").RemoveByPrefixAsync("a*b");

        server.Received(1).KeysAsync(Arg.Any<int>(), new RedisValue(@"svc\?\[1\]:test:a\*b*"), Arg.Any<int>(), Arg.Any<long>(),
            Arg.Any<int>(), Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task Container_WithRedisCacheOptions_UsesTheInstanceName()
    {
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(o => o.InstanceName = "app:");
        services.AddSingleton(_redis);
        services.AddSingleton<Microsoft.Extensions.Options.IOptionsMonitor<CacheOptions>>(
            new StaticOptionsMonitor<CacheOptions>(new CacheOptions { KeyPrefix = "test:" }));
        services.AddSingleton<RedisCacheService>();
        var db = Substitute.For<IDatabase>();
        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<RedisCacheService>().ExistsAsync("k");

        await db.Received(1).KeyExistsAsync(new RedisKey("app:test:k"), Arg.Any<CommandFlags>());
    }

    // K2 (2026-10-31 health analysis, probe P1): a miss on a value type returned default(T) and never ran the
    // factory, because "existing is not null" is always true for int/bool/Guid.
    [Fact]
    public async Task GetOrSetAsync_IntMiss_CallsFactoryOnceAndStoresValue()
    {
        _cache.GetAsync("test:vt:int", Arg.Any<CancellationToken>()).Returns(default(byte[]?));
        var calls = 0;

        var result = await _sut.GetOrSetAsync("vt:int", () => { calls++; return Task.FromResult(42); });

        result.Should().Be(42);
        calls.Should().Be(1);
        await _cache.Received(1).SetAsync(
            "test:vt:int",
            Arg.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "42"),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrSetAsync_BoolAndGuidMiss_ReturnTheFactoryValue()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(default(byte[]?));
        var guid = Guid.NewGuid();

        (await _sut.GetOrSetAsync("vt:bool", () => Task.FromResult(true))).Should().BeTrue();
        (await _sut.GetOrSetAsync("vt:guid", () => Task.FromResult(guid))).Should().Be(guid);
    }

    [Fact]
    public async Task GetOrSetAsync_StoredDefaultValue_IsAHit()
    {
        _cache.GetAsync("test:vt:zero", Arg.Any<CancellationToken>()).Returns(Encoding.UTF8.GetBytes("0"));
        var calls = 0;

        var result = await _sut.GetOrSetAsync("vt:zero", () => { calls++; return Task.FromResult(99); });

        result.Should().Be(0);
        calls.Should().Be(0, "0 is a stored value, not a miss");
    }

    private sealed record TestDto(string Name, int Value);
}
