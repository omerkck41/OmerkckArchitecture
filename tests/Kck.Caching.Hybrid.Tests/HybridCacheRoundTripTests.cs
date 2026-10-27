using System.Collections.Concurrent;
using AwesomeAssertions;
using Kck.Caching.Hybrid;
using Kck.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Kck.Caching.Hybrid.Tests;

/// <summary>
/// Uses the real Microsoft HybridCache (not a mock) so the L2 payload format is the one HybridCache writes.
/// L2 is a dictionary-backed <see cref="IDistributedCache"/>: HybridCache ignores MemoryDistributedCache.
/// </summary>
public sealed class HybridCacheRoundTripTests
{
    private sealed record TestDto(string Name, int Value);

    private sealed class DictionaryDistributedCache : IDistributedCache
    {
        public ConcurrentDictionary<string, byte[]> Entries { get; } = new();

        public byte[]? Get(string key) => Entries.TryGetValue(key, out var value) ? value : null;
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => Entries[key] = value;

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key) { }
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        public void Remove(string key) => Entries.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }

    private static (HybridCacheService Sut, ServiceProvider Provider) CreateSut(DictionaryDistributedCache l2)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDistributedCache>(l2);
        services.AddHybridCache();
        var provider = services.BuildServiceProvider();
        var options = new StaticOptionsMonitor<HybridCacheOptions>(new HybridCacheOptions { KeyPrefix = "test:" });
        var sut = new HybridCacheService(
            provider.GetRequiredService<HybridCache>(), Substitute.For<IConnectionMultiplexer>(), options);
        return (sut, provider);
    }

    [Fact]
    public async Task GetAsync_AfterSetAsync_ReturnsValue()
    {
        var (sut, provider) = CreateSut(new DictionaryDistributedCache());
        await using var _ = provider;

        await sut.SetAsync("dto", new TestDto("hello", 42));
        await sut.SetAsync("text", "shipped");
        await sut.SetAsync("number", 7);

        (await sut.GetAsync<TestDto>("dto")).Should().Be(new TestDto("hello", 42));
        (await sut.GetAsync<string>("text")).Should().Be("shipped");
        (await sut.GetAsync<int>("number")).Should().Be(7);
    }

    [Fact]
    public async Task GetAsync_ReadsValueWrittenByAnotherInstanceFromL2()
    {
        var l2 = new DictionaryDistributedCache();
        var (writer, writerProvider) = CreateSut(l2);
        await using (writerProvider)
        {
            await writer.SetAsync("order:42", "shipped");
        }

        // New HybridCache instance: empty L1, so the value can only come from the shared L2 store.
        var (reader, readerProvider) = CreateSut(l2);
        await using var _ = readerProvider;

        (await reader.GetAsync<string>("order:42")).Should().Be("shipped");
    }

    [Fact]
    public async Task GetAsync_WhenKeyMissing_ReturnsDefaultAndDoesNotStoreAnything()
    {
        var l2 = new DictionaryDistributedCache();
        var (sut, provider) = CreateSut(l2);
        await using var _ = provider;

        (await sut.GetAsync<TestDto>("missing")).Should().BeNull();
        (await sut.GetAsync<int>("missing-number")).Should().Be(0);

        l2.Entries.Should().BeEmpty();
    }
}
