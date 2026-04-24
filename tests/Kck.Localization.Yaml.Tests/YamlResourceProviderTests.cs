using FluentAssertions;
using Kck.Localization.Abstractions;
using Kck.Localization.Yaml;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Localization.Yaml.Tests;

public sealed class YamlResourceProviderTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly YamlResourceProvider _sut;

    public YamlResourceProviderTests()
    {
        Directory.CreateDirectory(_tempDir);

        var options = new StaticOptionsMonitor<LocalizationOptions>(new LocalizationOptions
        {
            ResourcePath = _tempDir,
            EnableCaching = false
        });

        _sut = new YamlResourceProvider(options, NullLogger<YamlResourceProvider>.Instance);
    }

    [Fact]
    public async Task GetStringAsync_ReturnsValue_WhenKeyExists()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "en.yaml"), "greeting: Hello");
        var result = await _sut.GetStringAsync("greeting", "en");
        result.Should().Be("Hello");
    }

    [Fact]
    public async Task GetStringAsync_ReturnsNull_WhenKeyMissing()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "en.yaml"), "greeting: Hello");
        var result = await _sut.GetStringAsync("missing", "en");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStringAsync_SupportsNestedKeys()
    {
        var yaml = "nav:\n  home: Home Page\n  about: About";
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "en.yaml"), yaml);
        var result = await _sut.GetStringAsync("nav.home", "en");
        result.Should().Be("Home Page");
    }

    [Fact]
    public async Task GetAllStringsAsync_ReturnsAllKeys()
    {
        var yaml = "a: \"1\"\nb: \"2\"";
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "en.yaml"), yaml);
        var result = await _sut.GetAllStringsAsync("en");
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStringAsync_ReturnsNull_WhenFileNotFound()
    {
        var result = await _sut.GetStringAsync("key", "xx");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStringAsync_SupportsYmlExtension()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "fr.yml"), "greeting: Bonjour");
        var result = await _sut.GetStringAsync("greeting", "fr");
        result.Should().Be("Bonjour");
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
        var sut = new YamlResourceProvider(options, NullLogger<YamlResourceProvider>.Instance);

        await File.WriteAllTextAsync(Path.Combine(_tempDir, "en.yaml"), "key: original");
        var first = await sut.GetStringAsync("key", "en");
        first.Should().Be("original");

        await File.WriteAllTextAsync(Path.Combine(_tempDir, "en.yaml"), "key: updated");
        await sut.ReloadAsync();
        var second = await sut.GetStringAsync("key", "en");
        second.Should().Be("updated");
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
