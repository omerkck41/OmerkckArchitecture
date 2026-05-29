using FluentAssertions;
using Kck.Security.Abstractions.Mfa;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kck.Security.Totp.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKckTotp_WithoutConfigure_RegistersProviderWithDefaults()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddKckTotp();
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IMfaProvider>().Should().BeOfType<TotpMfaProvider>();
        var options = provider.GetRequiredService<IOptionsMonitor<TotpOptions>>().CurrentValue;
        options.CodeLength.Should().Be(6);
        options.StepSeconds.Should().Be(30);
        options.VerificationWindow.Should().Be(1);
    }

    [Fact]
    public void AddKckTotp_WithConfigure_AppliesConfiguredOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddKckTotp(o =>
        {
            o.CodeLength = 8;
            o.StepSeconds = 60;
            o.VerificationWindow = 2;
        });
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IMfaProvider>().Should().BeOfType<TotpMfaProvider>();
        var options = provider.GetRequiredService<IOptionsMonitor<TotpOptions>>().CurrentValue;
        options.CodeLength.Should().Be(8);
        options.StepSeconds.Should().Be(60);
        options.VerificationWindow.Should().Be(2);
    }

    [Fact]
    public void AddKckTotp_RegistersMemoryCacheForReplayProtection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddKckTotp();
        using var provider = services.BuildServiceProvider();

        // Assert — replay protection requires a singleton IMemoryCache
        provider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>().Should().NotBeNull();
    }

    [Fact]
    public void AddKckTotp_ReturnsSameServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddKckTotp();

        // Assert
        result.Should().BeSameAs(services);
    }
}
