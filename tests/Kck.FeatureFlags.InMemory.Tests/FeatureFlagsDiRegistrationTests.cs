using FluentAssertions;
using Kck.FeatureFlags.Abstractions;
using Kck.FeatureFlags.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.FeatureFlags.InMemory.Tests;

public class FeatureFlagsDiRegistrationTests
{
    [Fact]
    public void AddKckFeatureFlagsInMemory_ShouldRegisterFeatureFlagService()
    {
        var services = new ServiceCollection();

        services.AddKckFeatureFlagsInMemory(opts =>
        {
            opts.Features["dark-mode"] = true;
        });

        using var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IFeatureFlagService>();

        svc.Should().BeOfType<InMemoryFeatureFlagService>();
    }

    [Fact]
    public async Task AddKckFeatureFlagsInMemory_ShouldApplyConfiguredFeatures()
    {
        var services = new ServiceCollection();

        services.AddKckFeatureFlagsInMemory(opts =>
        {
            opts.Features["enabled-flag"] = true;
            opts.Features["disabled-flag"] = false;
        });

        using var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IFeatureFlagService>();

        (await svc.IsEnabledAsync("enabled-flag")).Should().BeTrue();
        (await svc.IsEnabledAsync("disabled-flag")).Should().BeFalse();
    }

    [Fact]
    public void AddKckFeatureFlagsInMemory_WithoutConfigure_ShouldStillRegister()
    {
        var services = new ServiceCollection();

        services.AddKckFeatureFlagsInMemory();

        using var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<IFeatureFlagService>();

        svc.Should().NotBeNull();
    }

    [Fact]
    public void AddKckFeatureFlagsInMemory_TwiceRegistered_ShouldUseSingleton()
    {
        var services = new ServiceCollection();

        services.AddKckFeatureFlagsInMemory();
        services.AddKckFeatureFlagsInMemory();

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IFeatureFlagService>();
        var second = provider.GetRequiredService<IFeatureFlagService>();

        first.Should().BeSameAs(second);
    }
}
