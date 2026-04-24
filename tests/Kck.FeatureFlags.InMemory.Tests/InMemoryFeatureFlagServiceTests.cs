using FluentAssertions;
using Kck.FeatureFlags.Abstractions;
using Kck.FeatureFlags.InMemory;
using Kck.Testing;
using Xunit;

namespace Kck.FeatureFlags.InMemory.Tests;

public class InMemoryFeatureFlagServiceTests
{
    private static InMemoryFeatureFlagService CreateSut(Dictionary<string, bool>? features = null)
    {
        var options = new StaticOptionsMonitor<InMemoryFeatureFlagOptions>(new InMemoryFeatureFlagOptions
        {
            Features = features ?? new Dictionary<string, bool>()
        });
        return new InMemoryFeatureFlagService(options);
    }

    [Fact]
    public async Task IsEnabledAsync_KnownEnabledFeature_ShouldReturnTrue()
    {
        var sut = CreateSut(new Dictionary<string, bool> { ["new-checkout"] = true });

        var enabled = await sut.IsEnabledAsync("new-checkout");

        enabled.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_KnownDisabledFeature_ShouldReturnFalse()
    {
        var sut = CreateSut(new Dictionary<string, bool> { ["legacy-ui"] = false });

        var enabled = await sut.IsEnabledAsync("legacy-ui");

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_UnknownFeature_ShouldReturnFalse()
    {
        var sut = CreateSut();

        var enabled = await sut.IsEnabledAsync("missing-feature");

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_WithContext_ShouldBehaveSameAsWithout()
    {
        var sut = CreateSut(new Dictionary<string, bool> { ["beta"] = true });
        var context = new TestContext("user-1");

        var enabled = await sut.IsEnabledAsync("beta", context);

        enabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetValueAsync_ShouldAlwaysReturnDefault()
    {
        var sut = CreateSut(new Dictionary<string, bool> { ["flag"] = true });

        var result = await sut.GetValueAsync("flag", "default-value");

        result.Should().Be("default-value");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllConfiguredFeatures()
    {
        var sut = CreateSut(new Dictionary<string, bool>
        {
            ["a"] = true,
            ["b"] = false
        });

        var all = await sut.GetAllAsync();

        all.Should().HaveCount(2);
        all.Should().Contain(f => f.Name == "a" && f.Enabled);
        all.Should().Contain(f => f.Name == "b" && !f.Enabled);
    }

    [Fact]
    public async Task GetAllAsync_EmptyConfiguration_ShouldReturnEmptyList()
    {
        var sut = CreateSut();

        var all = await sut.GetAllAsync();

        all.Should().BeEmpty();
    }

    private sealed class TestContext(string userId) : IFeatureContext
    {
        public string? UserId { get; } = userId;
        public string? TenantId { get; }
        public IReadOnlyDictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    }
}
