using System.Collections.Concurrent;
using AwesomeAssertions;
using Kck.Caching.Abstractions;
using Xunit;

namespace Kck.Caching.InMemory.Tests;

/// <summary>
/// K2 for a provider that overrides only the abstract members of <see cref="CacheServiceBase"/> (a third-party
/// subclass): the base class's default <c>TryGetAsync</c> must not mistake a value-type miss for a hit either.
/// </summary>
public sealed class CacheServiceBaseDefaultTests
{
    // Minimal store: GetAsync returns default(T) for a missing key, exactly what such a subclass does.
    private sealed class DictionaryCacheService : CacheServiceBase
    {
        private readonly ConcurrentDictionary<string, object?> _store = new();

        protected override CacheOptions Options { get; } = new() { KeyPrefix = "t:" };

        public override ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : default =>
            ValueTask.FromResult(_store.TryGetValue(BuildKey(key), out var value) ? (T?)value : default);

        public override Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
        {
            _store[BuildKey(key)] = value;
            return Task.CompletedTask;
        }

        public override Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _store.TryRemove(BuildKey(key), out _);
            return Task.CompletedTask;
        }

        public override ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default) =>
            ValueTask.FromResult(_store.ContainsKey(BuildKey(key)));

        public override Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
        {
            foreach (var key in _store.Keys.Where(k => k.StartsWith(BuildKey(prefix), StringComparison.Ordinal)))
                _store.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task GetOrSetAsync_IntMiss_WithTheDefaultTryGet_RunsTheFactoryOnce()
    {
        var sut = new DictionaryCacheService();
        var calls = 0;

        var first = await sut.GetOrSetAsync("vt:int", () => { calls++; return Task.FromResult(42); });
        var second = await sut.GetOrSetAsync("vt:int", () => { calls++; return Task.FromResult(-1); });

        first.Should().Be(42);
        second.Should().Be(42);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task GetOrSetAsync_StoredDefaultValue_WithTheDefaultTryGet_IsAHit()
    {
        var sut = new DictionaryCacheService();
        await sut.SetAsync("vt:zero", 0);
        var calls = 0;

        var result = await sut.GetOrSetAsync("vt:zero", () => { calls++; return Task.FromResult(99); });

        result.Should().Be(0);
        calls.Should().Be(0, "0 is a stored value, not a miss");
    }

    [Fact]
    public async Task GetOrSetAsync_ReferenceTypeMiss_WithTheDefaultTryGet_RunsTheFactoryOnce()
    {
        var sut = new DictionaryCacheService();
        var calls = 0;

        var first = await sut.GetOrSetAsync("ref", () => { calls++; return Task.FromResult("v"); });
        var second = await sut.GetOrSetAsync("ref", () => { calls++; return Task.FromResult("w"); });

        first.Should().Be("v");
        second.Should().Be("v");
        calls.Should().Be(1);
    }
}
