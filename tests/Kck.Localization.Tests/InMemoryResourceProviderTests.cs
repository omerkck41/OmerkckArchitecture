using FluentAssertions;
using Kck.Localization.Abstractions;
using Xunit;

namespace Kck.Localization.Tests;

public sealed class InMemoryResourceProviderTests
{
    private readonly InMemoryResourceProvider _sut;

    public InMemoryResourceProviderTests()
    {
        var resources = new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new()
            {
                ["greeting"] = "Hello",
                ["farewell"] = "Goodbye",
                ["items.zero"] = "No items",
                ["items.one"] = "{0} item",
                ["items.other"] = "{0} items"
            },
            ["tr"] = new()
            {
                ["greeting"] = "Merhaba",
                ["farewell"] = "Hosca kal"
            }
        };

        _sut = new InMemoryResourceProvider(resources);
    }

    [Fact]
    public async Task GetStringAsync_ReturnsValue_WhenKeyExists()
    {
        var result = await _sut.GetStringAsync("greeting", "en");
        result.Should().Be("Hello");
    }

    [Fact]
    public async Task GetStringAsync_ReturnsNull_WhenKeyMissing()
    {
        var result = await _sut.GetStringAsync("nonexistent", "en");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStringAsync_ReturnsNull_WhenCultureMissing()
    {
        var result = await _sut.GetStringAsync("greeting", "fr");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllStringsAsync_ReturnsAllKeys_ForExistingCulture()
    {
        var result = await _sut.GetAllStringsAsync("en");
        result.Should().HaveCount(5);
        result["greeting"].Should().Be("Hello");
    }

    [Fact]
    public async Task GetAllStringsAsync_ReturnsEmpty_ForMissingCulture()
    {
        var result = await _sut.GetAllStringsAsync("fr");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task KeyExistsAsync_ReturnsTrue_WhenKeyExists()
    {
        var result = await _sut.KeyExistsAsync("greeting", "en");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task KeyExistsAsync_ReturnsFalse_WhenKeyMissing()
    {
        var result = await _sut.KeyExistsAsync("missing", "en");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReloadAsync_Completes_WithoutError()
    {
        var act = () => _sut.ReloadAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Priority_ReturnsDefault_Value()
    {
#pragma warning disable CA1859
        IResourceProvider provider = _sut;
#pragma warning restore CA1859
        provider.Priority.Should().Be(100);
    }

    [Fact]
    public void SupportsDynamicReload_ReturnsFalse()
    {
#pragma warning disable CA1859
        IResourceProvider provider2 = _sut;
#pragma warning restore CA1859
        provider2.SupportsDynamicReload.Should().BeFalse();
    }
}
