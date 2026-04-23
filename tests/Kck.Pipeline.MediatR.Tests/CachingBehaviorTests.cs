using System.Text.Json;
using FluentAssertions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Pipeline.MediatR.Behaviors;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Pipeline.MediatR.Tests;

public class CachingBehaviorTests
{
    private record TestCachableRequest(string CacheKey, bool BypassCache = false) : IRequest<TestResponse>, ICachableRequest
    {
        public string? CacheGroupKey => null;
        public TimeSpan? SlidingExpiration => null;
    }

    private record TestResponse(string Value);

    private readonly IDistributedCache _cache;
    private readonly CachingBehavior<TestCachableRequest, TestResponse> _sut;

    public CachingBehaviorTests()
    {
        _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var logger = NullLogger<CachingBehavior<TestCachableRequest, TestResponse>>.Instance;
        _sut = new CachingBehavior<TestCachableRequest, TestResponse>(_cache, logger);
    }

    [Fact]
    public async Task Handle_CacheMiss_ShouldCallNextAndCache()
    {
        // Arrange
        var request = new TestCachableRequest("test-key");
        var expected = new TestResponse("from-handler");
        var nextCalled = false;

        Task<TestResponse> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult(expected);
        }

        // Act
        var result = await _sut.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expected);
        nextCalled.Should().BeTrue();

        var cached = await _cache.GetStringAsync("test-key");
        cached.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_CacheHit_ShouldReturnCachedValue()
    {
        // Arrange
        var request = new TestCachableRequest("hit-key");
        var cachedResponse = new TestResponse("cached-value");

        await _cache.SetStringAsync("hit-key", JsonSerializer.Serialize(cachedResponse),
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });

        var nextCalled = false;

        // Act
        var result = await _sut.Handle(request, _ =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse("fresh"));
        }, CancellationToken.None);

        // Assert
        result.Value.Should().Be("cached-value");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_BypassCache_ShouldAlwaysCallNext()
    {
        // Arrange — once cache'e yaz
        var cacheKey = "bypass-key";
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(new TestResponse("old")),
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });

        var request = new TestCachableRequest(cacheKey, BypassCache: true);
        var nextCalled = false;

        // Act
        var result = await _sut.Handle(request, _ =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse("fresh"));
        }, CancellationToken.None);

        // Assert
        result.Value.Should().Be("fresh");
        nextCalled.Should().BeTrue();
    }
}
