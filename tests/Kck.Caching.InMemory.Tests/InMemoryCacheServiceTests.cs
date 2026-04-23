using FluentAssertions;
using Kck.Caching.Abstractions;
using Kck.Caching.InMemory;
using Kck.Testing;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Kck.Caching.InMemory.Tests;

public class InMemoryCacheServiceTests
{
    private readonly InMemoryCacheService _sut;

    public InMemoryCacheServiceTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = new StaticOptionsMonitor<CacheOptions>(new CacheOptions { KeyPrefix = "test:" });
        _sut = new InMemoryCacheService(memoryCache, options);
    }

    [Fact]
    public async Task GetAsync_NonExistingKey_ShouldReturnDefault()
    {
        // Act
        var result = await _sut.GetAsync<string>("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ShouldReturnValue()
    {
        // Arrange
        await _sut.SetAsync("key1", "value1");

        // Act
        var result = await _sut.GetAsync<string>("key1");

        // Assert
        result.Should().Be("value1");
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteKey()
    {
        // Arrange
        await _sut.SetAsync("key2", "value2");

        // Act
        await _sut.RemoveAsync("key2");
        var result = await _sut.GetAsync<string>("key2");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ShouldReturnTrue()
    {
        // Arrange
        await _sut.SetAsync("exists-key", "value");

        // Act
        var result = await _sut.ExistsAsync("exists-key");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistingKey_ShouldReturnFalse()
    {
        // Act
        var result = await _sut.ExistsAsync("no-key");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_CacheMiss_ShouldCallFactory()
    {
        // Arrange
        var factoryCalled = false;

        // Act
        var result = await _sut.GetOrSetAsync("factory-key", () =>
        {
            factoryCalled = true;
            return Task.FromResult("factory-value");
        });

        // Assert
        result.Should().Be("factory-value");
        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrSetAsync_CacheHit_ShouldNotCallFactory()
    {
        // Arrange
        await _sut.SetAsync("cached-key", "cached-value");
        var factoryCalled = false;

        // Act
        var result = await _sut.GetOrSetAsync("cached-key", () =>
        {
            factoryCalled = true;
            return Task.FromResult("fresh-value");
        });

        // Assert
        result.Should().Be("cached-value");
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_ShouldRespectTTL()
    {
        // Arrange
        await _sut.SetAsync("ttl-key", "value", TimeSpan.FromMilliseconds(50));

        // Act
        await Task.Delay(100);
        var result = await _sut.GetAsync<string>("ttl-key");

        // Assert
        result.Should().BeNull("cache entry should have expired");
    }

    [Fact]
    public async Task SetAsync_ComplexObject_ShouldStoreAndRetrieve()
    {
        // Arrange
        var obj = new TestCacheItem { Id = 1, Name = "Test" };

        // Act
        await _sut.SetAsync("complex-key", obj);
        var result = await _sut.GetAsync<TestCacheItem>("complex-key");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetOrSetAsync_ConcurrentCalls_ShouldCallFactoryOnlyOnce()
    {
        // Arrange
        var callCount = 0;
        var barrier = new Barrier(10);

        // Act
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
        {
            barrier.SignalAndWait();
            return await _sut.GetOrSetAsync("stampede-key", async () =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(50);
                return "stampede-value";
            });
        }));

        var results = await Task.WhenAll(tasks);

        // Assert
        callCount.Should().Be(1, "factory should be called exactly once");
        results.Should().AllBe("stampede-value");
    }

    [Fact]
    public async Task RemoveByPrefixAsync_ShouldRemoveMatchingKeys()
    {
        // Arrange
        await _sut.SetAsync("products:1", "p1");
        await _sut.SetAsync("products:2", "p2");
        await _sut.SetAsync("users:1", "u1");

        // Act
        await _sut.RemoveByPrefixAsync("products:");

        // Assert
        (await _sut.ExistsAsync("products:1")).Should().BeFalse();
        (await _sut.ExistsAsync("products:2")).Should().BeFalse();
        (await _sut.ExistsAsync("users:1")).Should().BeTrue("unrelated keys should not be removed");
    }

    [Fact]
    public async Task RemoveByPrefixAsync_EmptyPrefix_ShouldNotThrow()
    {
        // Arrange
        await _sut.SetAsync("keep-me", "value");

        // Act
        await _sut.RemoveByPrefixAsync("nonexistent:");

        // Assert
        (await _sut.ExistsAsync("keep-me")).Should().BeTrue();
    }

    private sealed class TestCacheItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }
}
