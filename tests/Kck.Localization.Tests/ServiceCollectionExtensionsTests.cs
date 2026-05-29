using FluentAssertions;
using Kck.Localization;
using Kck.Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Localization.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKckLocalization_WithoutConfigure_RegistersCoreServicesWithDefaults()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckLocalization();
        services.AddKckLocalizationInMemory(new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new() { ["k"] = "v" }
        });
        using var provider = services.BuildServiceProvider();

        provider.GetService<IPluralizer>().Should().BeOfType<DefaultPluralizer>();
        provider.GetService<IFormatterService>().Should().BeOfType<FormatterService>();
        provider.GetService<ILocalizationService>().Should().BeOfType<LocalizationService>();

        var options = provider.GetRequiredService<IOptionsMonitor<LocalizationOptions>>().CurrentValue;
        options.DefaultCulture.Should().Be("en");
    }

    [Fact]
    public void AddKckLocalization_WithConfigure_AppliesConfiguredOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckLocalization(o =>
        {
            o.DefaultCulture = "tr";
            o.FallbackCulture = "en";
            o.ThrowOnMissing = true;
        });
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptionsMonitor<LocalizationOptions>>().CurrentValue;
        options.DefaultCulture.Should().Be("tr");
        options.FallbackCulture.Should().Be("en");
        options.ThrowOnMissing.Should().BeTrue();
    }

    [Fact]
    public async Task AddKckLocalizationInMemory_WithDictionary_RegistersResolvableProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckLocalization();
        services.AddKckLocalizationInMemory(new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new() { ["greeting"] = "Hello" }
        });
        using var provider = services.BuildServiceProvider();

        var localization = provider.GetRequiredService<ILocalizationService>();
        (await localization.GetStringAsync("greeting", "en")).Should().Be("Hello");
    }

    [Fact]
    public async Task AddKckLocalizationInMemory_WithConfigureAction_RegistersResolvableProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckLocalization();
        services.AddKckLocalizationInMemory(p =>
            p.SetStrings("en", new Dictionary<string, string> { ["greeting"] = "Hi" }));
        using var provider = services.BuildServiceProvider();

        var localization = provider.GetRequiredService<ILocalizationService>();
        (await localization.GetStringAsync("greeting", "en")).Should().Be("Hi");
    }

    [Fact]
    public void AddKckLocalization_ReturnsSameServiceCollectionForChaining()
    {
        var services = new ServiceCollection();

        services.AddKckLocalization().Should().BeSameAs(services);
    }

    [Fact]
    public void AddKckLocalization_WithoutConfigure_StillRegistersOptionsInfrastructure()
    {
        // No AddLogging / no other AddOptions: the else-branch empty Configure call is the
        // only thing wiring up the options system, so IOptionsMonitor must resolve from it alone.
        var services = new ServiceCollection();
        services.AddKckLocalization();
        using var provider = services.BuildServiceProvider();

        provider.GetService<IOptionsMonitor<LocalizationOptions>>().Should().NotBeNull();
    }
}
