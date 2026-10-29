using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Kck.Pipeline.Abstractions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Pipeline.Mediator.Tests;

// Native AOT apps pass source-generated JSON metadata instead of relying on reflection.
public sealed partial class CachingBehaviorSerializerTests
{
    public sealed record CachedCount(int Count);

    public sealed record CountQuery(string Key) : IRequest<CachedCount>, ICachableRequest
    {
        public string CacheKey => Key;
        public bool BypassCache => false;
        public string? CacheGroupKey => null;
        public TimeSpan? SlidingExpiration => null;
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(CachedCount))]
    internal sealed partial class CountJsonContext : JsonSerializerContext;

    [Fact]
    public async Task Handle_WithSerializerOptions_StoresAndReadsThroughThem()
    {
        var cache = new MemoryDistributedCache(Options.Create(new Microsoft.Extensions.Caching.Memory.MemoryDistributedCacheOptions()));
        var sut = new CachingBehavior<CountQuery, CachedCount>(
            cache,
            NullLogger<CachingBehavior<CountQuery, CachedCount>>.Instance,
            Options.Create(new CachingBehaviorOptions { SerializerOptions = CountJsonContext.Default.Options }));

        await sut.Handle(new CountQuery("k"), (_, _) => ValueTask.FromResult(new CachedCount(3)), CancellationToken.None);
        var cached = await sut.Handle(new CountQuery("k"), (_, _) => ValueTask.FromResult(new CachedCount(9)), CancellationToken.None);

        (await cache.GetStringAsync("k")).Should().Be("""{"count":3}""");
        cached.Should().Be(new CachedCount(3));
    }

    [Fact]
    public void UseCachingBehavior_WithConfigure_BehaviorUsesTheConfiguredOptions()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddDistributedMemoryCache();
        new KckMediatorPipelineBuilder(services)
            .UseCachingBehavior(o => o.SerializerOptions = CountJsonContext.Default.Options);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CachingBehaviorOptions>>().Value;
        var behavior = provider.GetServices<IPipelineBehavior<CountQuery, CachedCount>>();

        options.SerializerOptions.Should().BeSameAs(CountJsonContext.Default.Options);
        behavior.Should().ContainSingle().Which.Should().BeOfType<CachingBehavior<CountQuery, CachedCount>>();
    }

    [Fact]
    public async Task Handle_WithoutOptions_UsesDefaultJson()
    {
        var cache = new MemoryDistributedCache(Options.Create(new Microsoft.Extensions.Caching.Memory.MemoryDistributedCacheOptions()));
        var sut = new CachingBehavior<CountQuery, CachedCount>(cache, NullLogger<CachingBehavior<CountQuery, CachedCount>>.Instance);

        await sut.Handle(new CountQuery("d"), (_, _) => ValueTask.FromResult(new CachedCount(5)), CancellationToken.None);

        (await cache.GetStringAsync("d")).Should().Be(JsonSerializer.Serialize(new CachedCount(5)));
    }
}
