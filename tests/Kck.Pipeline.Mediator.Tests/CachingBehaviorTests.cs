using System.Text.Json;
using AwesomeAssertions;
using Kck.Pipeline.Abstractions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class CachingBehaviorTests
{
    private sealed record CachableMessage(string Key, bool Bypass = false) : IRequest<string>, ICachableRequest
    {
        public string CacheKey => Key;
        public bool BypassCache => Bypass;
        public string? CacheGroupKey => null;
        public TimeSpan? SlidingExpiration => null;
    }

    private static MemoryDistributedCache BuildCache() =>
        new(Options.Create(new MemoryDistributedCacheOptions()));

    [Fact]
    public async Task Handle_CacheMiss_ShouldCallNextAndCache()
    {
        var cache = BuildCache();
        var sut = new CachingBehavior<CachableMessage, string>(
            cache, NullLogger<CachingBehavior<CachableMessage, string>>.Instance);
        var nextCalled = false;

        var result = await sut.Handle(
            new CachableMessage("key1"),
            (_, _) => { nextCalled = true; return ValueTask.FromResult("computed"); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("computed");
        var stored = await cache.GetStringAsync("key1");
        JsonSerializer.Deserialize<string>(stored!).Should().Be("computed");
    }

    [Fact]
    public async Task Handle_CacheHit_ShouldReturnCachedValueWithoutCallingNext()
    {
        var cache = BuildCache();
        await cache.SetStringAsync("key2", JsonSerializer.Serialize("cached"), new DistributedCacheEntryOptions());
        var sut = new CachingBehavior<CachableMessage, string>(
            cache, NullLogger<CachingBehavior<CachableMessage, string>>.Instance);
        var nextCalled = false;

        var result = await sut.Handle(
            new CachableMessage("key2"),
            (_, _) => { nextCalled = true; return ValueTask.FromResult("fresh"); },
            CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.Should().Be("cached");
    }

    [Fact]
    public async Task Handle_BypassCache_ShouldAlwaysCallNext()
    {
        var cache = BuildCache();
        await cache.SetStringAsync("key3", JsonSerializer.Serialize("cached"), new DistributedCacheEntryOptions());
        var sut = new CachingBehavior<CachableMessage, string>(
            cache, NullLogger<CachingBehavior<CachableMessage, string>>.Instance);

        var result = await sut.Handle(
            new CachableMessage("key3", Bypass: true),
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        result.Should().Be("fresh");
    }
}
