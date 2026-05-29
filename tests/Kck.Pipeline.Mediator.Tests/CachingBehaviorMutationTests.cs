using System.Text.Json;
using AwesomeAssertions;
using Kck.Pipeline.Abstractions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Pipeline.Mediator.Tests;

/// <summary>
/// Targets caching mutants the happy-path suite left alive: hit/set logging and the
/// sliding-expiration null-coalescing (message value vs the 5-minute default).
/// </summary>
public sealed class CachingBehaviorMutationTests
{
    private sealed record CachableMessage(string Key, TimeSpan? Sliding = null) : IRequest<string>, ICachableRequest
    {
        public string CacheKey => Key;
        public bool BypassCache => false;
        public string? CacheGroupKey => null;
        public TimeSpan? SlidingExpiration => Sliding;
    }

    [Fact]
    public async Task Handle_CacheHit_LogsCacheHit()
    {
        var cache = new CapturingDistributedCache();
        await cache.SetStringAsync("k", JsonSerializer.Serialize("cached"), new DistributedCacheEntryOptions());
        var logger = new CapturingLogger<CachingBehavior<CachableMessage, string>>();
        var sut = new CachingBehavior<CachableMessage, string>(cache, logger);

        var result = await sut.Handle(new CachableMessage("k"), (_, _) => ValueTask.FromResult("fresh"), CancellationToken.None);

        result.Should().Be("cached");
        logger.Entries.Should().Contain(e => e.Contains("Cache hit"));
    }

    [Fact]
    public async Task Handle_CacheMiss_LogsCacheSet_AndUsesDefaultExpiration()
    {
        var cache = new CapturingDistributedCache();
        var logger = new CapturingLogger<CachingBehavior<CachableMessage, string>>();
        var sut = new CachingBehavior<CachableMessage, string>(cache, logger);

        await sut.Handle(new CachableMessage("k"), (_, _) => ValueTask.FromResult("fresh"), CancellationToken.None);

        logger.Entries.Should().Contain(e => e.Contains("Cache set"));
        cache.LastSetOptions.Should().NotBeNull();
        cache.LastSetOptions!.SlidingExpiration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Handle_CacheMiss_UsesMessageSlidingExpirationWhenProvided()
    {
        var cache = new CapturingDistributedCache();
        var sut = new CachingBehavior<CachableMessage, string>(
            cache, NullLogger<CachingBehavior<CachableMessage, string>>.Instance);

        await sut.Handle(
            new CachableMessage("k", TimeSpan.FromMinutes(10)),
            (_, _) => ValueTask.FromResult("fresh"),
            CancellationToken.None);

        cache.LastSetOptions!.SlidingExpiration.Should().Be(TimeSpan.FromMinutes(10));
    }

    private sealed class CapturingDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _store = [];
        public DistributedCacheEntryOptions? LastSetOptions { get; private set; }

        public byte[]? Get(string key) => _store.TryGetValue(key, out var v) ? v : null;
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _store[key] = value;
            LastSetOptions = options;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key) { }
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            _store.Remove(key);
            return Task.CompletedTask;
        }
    }
}
