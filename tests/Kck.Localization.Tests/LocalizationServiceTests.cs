using FluentAssertions;
using Kck.Localization.Abstractions;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Localization.Tests;

public sealed class LocalizationServiceTests
{
    private readonly InMemoryResourceProvider _primaryProvider;
    private readonly InMemoryResourceProvider _fallbackProvider;
    private readonly LocalizationService _sut;

    public LocalizationServiceTests()
    {
        _primaryProvider = new InMemoryResourceProvider(new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new()
            {
                ["greeting"] = "Hello",
                ["farewell"] = "Goodbye",
                ["welcome"] = "Welcome, {0}!",
                ["items.one"] = "{0} item",
                ["items.other"] = "{0} items"
            },
            ["tr"] = new()
            {
                ["greeting"] = "Merhaba"
            }
        });

        _fallbackProvider = new InMemoryResourceProvider(new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new()
            {
                ["footer"] = "Footer text"
            }
        });

        var options = new LocalizationOptions
        {
            DefaultCulture = "en",
            FallbackCulture = "en",
            SupportedCultures = ["en", "tr"],
            ThrowOnMissing = false,
            MissingKeyPattern = "[{0}]"
        };

        _sut = new LocalizationService(
            [_primaryProvider, _fallbackProvider],
            new DefaultPluralizer(),
            new StaticOptionsMonitor<LocalizationOptions>(options),
            NullLogger<LocalizationService>.Instance);
    }

    [Fact]
    public async Task GetStringAsync_ByKey_ReturnsFromDefaultCulture()
    {
        var result = await _sut.GetStringAsync("greeting");
        result.Should().Be("Hello");
    }

    [Fact]
    public async Task GetStringAsync_WithCulture_ReturnsFromSpecifiedCulture()
    {
        var result = await _sut.GetStringAsync("greeting", "tr");
        result.Should().Be("Merhaba");
    }

    [Fact]
    public async Task GetStringAsync_WithArgs_FormatsString()
    {
        var result = await _sut.GetStringAsync("welcome", "en", ["World"]);
        result.Should().Be("Welcome, World!");
    }

    [Fact]
    public async Task GetStringAsync_MissingInCulture_FallsBackToFallbackCulture()
    {
        var result = await _sut.GetStringAsync("farewell", "tr");
        result.Should().Be("Goodbye");
    }

    [Fact]
    public async Task GetStringAsync_MissingEverywhere_ReturnsMissingKeyPattern()
    {
        var result = await _sut.GetStringAsync("nonexistent", "en");
        result.Should().Be("[nonexistent]");
    }

    [Fact]
    public async Task GetStringAsync_QueriesMultipleProviders()
    {
        var result = await _sut.GetStringAsync("footer", "en");
        result.Should().Be("Footer text");
    }

    [Fact]
    public async Task GetStringAsync_ThrowOnMissing_ThrowsKeyNotFoundException()
    {
        var throwOptions = new LocalizationOptions
        {
            DefaultCulture = "en",
            FallbackCulture = "en",
            ThrowOnMissing = true
        };

        var sut = new LocalizationService(
            [_primaryProvider],
            new DefaultPluralizer(),
            new StaticOptionsMonitor<LocalizationOptions>(throwOptions),
            NullLogger<LocalizationService>.Instance);

        var act = () => sut.GetStringAsync("nonexistent", "en");
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task TryGetStringAsync_ReturnsNull_WhenKeyMissing()
    {
        var result = await _sut.TryGetStringAsync("nonexistent", "en");
        result.Should().BeNull();
    }

    [Fact]
    public async Task TryGetStringAsync_ReturnsValue_WhenKeyExists()
    {
        var result = await _sut.TryGetStringAsync("greeting", "en");
        result.Should().Be("Hello");
    }

    [Fact]
    public async Task GetPluralStringAsync_ReturnsOne_WhenCountIsOne()
    {
        var result = await _sut.GetPluralStringAsync("items", 1, "en");
        result.Should().Be("1 item");
    }

    [Fact]
    public async Task GetPluralStringAsync_ReturnsOther_WhenCountIsMultiple()
    {
        var result = await _sut.GetPluralStringAsync("items", 5, "en");
        result.Should().Be("5 items");
    }

    [Fact]
    public async Task GetPluralStringAsync_FallsBackToOther_WhenCategoryMissing()
    {
        // "zero" category missing, falls back to "other"
        var result = await _sut.GetPluralStringAsync("items", 0, "en");
        result.Should().Be("0 items");
    }

    [Fact]
    public async Task GetAllStringsAsync_MergesAllProviders()
    {
        var result = await _sut.GetAllStringsAsync("en");
        result.Should().ContainKey("greeting");
        result.Should().ContainKey("footer");
    }

    [Fact]
    public async Task GetAllKeysAsync_ReturnsDistinctKeys()
    {
        var result = await _sut.GetAllKeysAsync("en");
        result.Should().Contain("greeting");
        result.Should().Contain("footer");
        result.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ReloadAsync_DoesNotThrow()
    {
        var act = () => _sut.ReloadAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetStringAsync_ParentCultureFallback_ResolvesFromParent()
    {
        // "en-US" should fall back to "en" (parent culture)
        var result = await _sut.GetStringAsync("greeting", "en-US");
        result.Should().Be("Hello");
    }

    [Fact]
    public async Task GetPluralStringAsync_ThrowOnMissing_ThrowsKeyNotFoundException()
    {
        var throwOptions = new LocalizationOptions
        {
            DefaultCulture = "en",
            FallbackCulture = "en",
            ThrowOnMissing = true
        };

        var sut = new LocalizationService(
            [_primaryProvider],
            new DefaultPluralizer(),
            new StaticOptionsMonitor<LocalizationOptions>(throwOptions),
            NullLogger<LocalizationService>.Instance);

        var act = () => sut.GetPluralStringAsync("nonexistent", 5, "en");
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
