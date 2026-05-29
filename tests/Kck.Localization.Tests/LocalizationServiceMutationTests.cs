using AwesomeAssertions;
using Kck.Localization.Abstractions;
using Kck.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Localization.Tests;

/// <summary>
/// Behaviour-distinguishing tests targeting mutants that survived the happy-path suite:
/// provider priority ordering, distinct fallback stages, arg formatting, plural fallback,
/// dynamic-reload filtering and missing-key logging.
/// </summary>
public sealed class LocalizationServiceMutationTests
{
    private static LocalizationService BuildSut(LocalizationOptions options, ILogger<LocalizationService>? logger, params IResourceProvider[] providers) =>
        new(providers,
            new DefaultPluralizer(),
            new StaticOptionsMonitor<LocalizationOptions>(options),
            logger ?? NullLogger<LocalizationService>.Instance);

    private static LocalizationOptions Options(string fallback = "en") => new()
    {
        DefaultCulture = "en",
        FallbackCulture = fallback,
        ThrowOnMissing = false,
        MissingKeyPattern = "[{0}]"
    };

    // --- Provider priority ordering ----------------------------------------

    [Fact]
    public async Task GetStringAsync_HigherPriorityProviderWins_ForOverlappingKey()
    {
        var high = new FakeResourceProvider(priority: 10, data: new() { ["en"] = new() { ["dup"] = "HIGH" } });
        var low = new FakeResourceProvider(priority: 20, data: new() { ["en"] = new() { ["dup"] = "LOW" } });

        // Registered low-first so resolution depends on priority sort, not insertion order.
        var sut = BuildSut(Options(), null, low, high);

        var result = await sut.GetStringAsync("dup", "en");

        result.Should().Be("HIGH", "lower Priority value is queried first");
    }

    [Fact]
    public async Task GetAllStringsAsync_HigherPriorityProviderOverwrites_ForOverlappingKey()
    {
        var high = new FakeResourceProvider(priority: 10, data: new() { ["en"] = new() { ["dup"] = "HIGH" } });
        var low = new FakeResourceProvider(priority: 20, data: new() { ["en"] = new() { ["dup"] = "LOW" } });

        var sut = BuildSut(Options(), null, low, high);

        var merged = await sut.GetAllStringsAsync("en");

        merged["dup"].Should().Be("HIGH", "higher-priority provider overwrites when merging");
    }

    // --- Distinct fallback stages ------------------------------------------

    [Fact]
    public async Task GetStringAsync_ResolvesViaParentCulture_WhenExactMissing()
    {
        // Key only in parent "en"; fallback culture is distinct ("zz") so only the parent stage can resolve.
        var provider = new FakeResourceProvider(data: new() { ["en"] = new() { ["k"] = "parent-value" } });
        var sut = BuildSut(Options(fallback: "zz"), null, provider);

        var result = await sut.GetStringAsync("k", "en-US");

        result.Should().Be("parent-value");
    }

    [Fact]
    public async Task GetStringAsync_ResolvesViaFallbackCulture_WhenExactAndParentMissing()
    {
        // "de" has an invariant parent (no parent stage match); key lives only in the fallback culture "fr".
        var provider = new FakeResourceProvider(data: new() { ["fr"] = new() { ["k"] = "fallback-value" } });
        var sut = BuildSut(Options(fallback: "fr"), null, provider);

        var result = await sut.GetStringAsync("k", "de");

        result.Should().Be("fallback-value");
    }

    [Fact]
    public async Task GetStringAsync_ParentEqualsInvariant_DoesNotResolveFromEmptyCulture()
    {
        // "de" parent is invariant -> GetParentCulture returns null -> parent stage is skipped.
        // Key only under exact "de"; assert it still resolves (parent stage must not short-circuit it).
        var provider = new FakeResourceProvider(data: new() { ["de"] = new() { ["k"] = "exact-de" } });
        var sut = BuildSut(Options(fallback: "zz"), null, provider);

        var result = await sut.GetStringAsync("k", "de");

        result.Should().Be("exact-de");
    }

    [Fact]
    public async Task GetStringAsync_InvalidCulture_CatchesParentLookupFailureAndFallsThrough()
    {
        // "a b c" is not a valid culture name: GetParentCulture must catch CultureNotFoundException
        // and return null, letting resolution fall through to the fallback culture.
        var provider = new FakeResourceProvider(data: new() { ["fr"] = new() { ["k"] = "fallback-value" } });
        var sut = BuildSut(Options(fallback: "fr"), null, provider);

        var result = await sut.GetStringAsync("k", "a b c");

        result.Should().Be("fallback-value");
    }

    // --- Argument formatting -----------------------------------------------

    [Fact]
    public async Task GetStringAsync_NoArgs_ReturnsRawValueWithoutFormatting()
    {
        // Value contains a placeholder but no args are supplied: must return the raw template,
        // not attempt string.Format (which would throw on the missing argument).
        var provider = new FakeResourceProvider(data: new() { ["en"] = new() { ["msg"] = "Hi {0}" } });
        var sut = BuildSut(Options(), null, provider);

        var result = await sut.GetStringAsync("msg", "en");

        result.Should().Be("Hi {0}");
    }

    // --- Plural fallback to "other" ----------------------------------------

    [Fact]
    public async Task GetPluralStringAsync_FallsBackToOther_WhenSpecificCategoryMissing()
    {
        // count=1 -> category "one", but only "f.other" exists -> must fall back to "other".
        var provider = new FakeResourceProvider(data: new() { ["en"] = new() { ["f.other"] = "{0} x" } });
        var sut = BuildSut(Options(), null, provider);

        var result = await sut.GetPluralStringAsync("f", 1, "en");

        result.Should().Be("1 x");
    }

    // --- Dynamic reload filtering ------------------------------------------

    [Fact]
    public async Task ReloadAsync_OnlyReloadsProvidersSupportingDynamicReload()
    {
        var reloadable = new FakeResourceProvider(supportsReload: true);
        var fixedProvider = new FakeResourceProvider(supportsReload: false);
        var sut = BuildSut(Options(), null, reloadable, fixedProvider);

        await sut.ReloadAsync();

        reloadable.ReloadCount.Should().Be(1);
        fixedProvider.ReloadCount.Should().Be(0);
    }

    // --- Missing-key logging -----------------------------------------------

    [Fact]
    public async Task GetStringAsync_MissingKey_LogsDebugMessage()
    {
        var logger = new CapturingLogger<LocalizationService>();
        var provider = new FakeResourceProvider(data: new() { ["en"] = new() { ["present"] = "ok" } });
        var sut = BuildSut(Options(fallback: "en"), logger, provider);

        await sut.GetStringAsync("absent", "en");

        logger.Entries.Should().ContainSingle(e => e.Contains("absent") && e.Contains("en"));
    }

    // --- Test doubles ------------------------------------------------------

    private sealed class FakeResourceProvider(
        int priority = 100,
        bool supportsReload = false,
        Dictionary<string, Dictionary<string, string>>? data = null) : IResourceProvider
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data = data ?? [];

        public int Priority { get; } = priority;
        public bool SupportsDynamicReload { get; } = supportsReload;
        public int ReloadCount { get; private set; }

        public Task<string?> GetStringAsync(string key, string culture, CancellationToken ct = default) =>
            Task.FromResult(_data.TryGetValue(culture, out var d) && d.TryGetValue(key, out var v) ? v : null);

        public Task<IReadOnlyDictionary<string, string>> GetAllStringsAsync(string culture, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyDictionary<string, string>>(
                _data.TryGetValue(culture, out var d) ? d : new Dictionary<string, string>());

        public Task<bool> KeyExistsAsync(string key, string culture, CancellationToken ct = default) =>
            Task.FromResult(_data.TryGetValue(culture, out var d) && d.ContainsKey(key));

        public Task ReloadAsync(CancellationToken ct = default)
        {
            ReloadCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add(formatter(state, exception));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
