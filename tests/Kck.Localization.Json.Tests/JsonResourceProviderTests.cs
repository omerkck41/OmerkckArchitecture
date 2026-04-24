using FluentAssertions;
using Kck.Localization.Abstractions;
using Kck.Localization.Json;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Localization.Json.Tests;

public sealed class JsonResourceProviderTests : IDisposable
{
    private readonly string _tempDir = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly JsonResourceProvider _sut;

    public JsonResourceProviderTests()
    {
        Directory.CreateDirectory(_tempDir);

        var options = new StaticOptionsMonitor<LocalizationOptions>(new LocalizationOptions
        {
            ResourcePath = _tempDir,
            EnableCaching = false
        });

        _sut = new JsonResourceProvider(options, NullLogger<JsonResourceProvider>.Instance);
    }

    [Fact]
    public async Task GetStringAsync_ReturnsValue_WhenKeyExists()
    {
        await File.WriteAllTextAsync(Path.Join(_tempDir, "en.json"), """{"greeting": "Hello"}""");

        var result = await _sut.GetStringAsync("greeting", "en");

        result.Should().Be("Hello");
    }

    [Fact]
    public async Task GetStringAsync_ReturnsNull_WhenKeyMissing()
    {
        await File.WriteAllTextAsync(Path.Join(_tempDir, "en.json"), """{"greeting": "Hello"}""");

        var result = await _sut.GetStringAsync("missing", "en");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStringAsync_SupportsNestedKeys()
    {
        await File.WriteAllTextAsync(Path.Join(_tempDir, "en.json"),
            """{"nav": {"home": "Home Page"}}""");

        var result = await _sut.GetStringAsync("nav.home", "en");

        result.Should().Be("Home Page");
    }

    [Fact]
    public async Task GetAllStringsAsync_ReturnsAllKeys()
    {
        await File.WriteAllTextAsync(Path.Join(_tempDir, "en.json"),
            """{"a": "1", "b": "2"}""");

        var result = await _sut.GetAllStringsAsync("en");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStringAsync_ReturnsEmpty_WhenFileNotFound()
    {
        var result = await _sut.GetStringAsync("key", "xx");

        result.Should().BeNull();
    }

    [Fact]
    public void Priority_ReturnsDefaultValue()
    {
        _sut.Priority.Should().Be(100);
    }

    [Fact]
    public void SupportsDynamicReload_ReturnsTrue()
    {
        _sut.SupportsDynamicReload.Should().BeTrue();
    }

    [Fact]
    public async Task ReloadAsync_ClearsCache_SubsequentReadLoadsFromDisk()
    {
        var options = new StaticOptionsMonitor<LocalizationOptions>(new LocalizationOptions
        {
            ResourcePath = _tempDir,
            EnableCaching = true
        });
        var sut = new JsonResourceProvider(options, NullLogger<JsonResourceProvider>.Instance);

        await File.WriteAllTextAsync(Path.Join(_tempDir, "en.json"), """{"key": "original"}""");
        var first = await sut.GetStringAsync("key", "en");
        first.Should().Be("original");

        await File.WriteAllTextAsync(Path.Join(_tempDir, "en.json"), """{"key": "updated"}""");
        await sut.ReloadAsync();
        var second = await sut.GetStringAsync("key", "en");

        second.Should().Be("updated");
    }

    [Fact]
    public void Priority_WithCustomValue_ReturnsConfiguredPriority()
    {
        var options = new StaticOptionsMonitor<LocalizationOptions>(new LocalizationOptions
        {
            ResourcePath = _tempDir,
            EnableCaching = false
        });
        var sut = new JsonResourceProvider(options, NullLogger<JsonResourceProvider>.Instance, priority: 50);

        sut.Priority.Should().Be(50);
    }

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetStringAsync_PathTraversal_ReturnsNull(string maliciousCulture)
    {
        var result = await _sut.GetStringAsync("key", maliciousCulture);
        result.Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }
}
