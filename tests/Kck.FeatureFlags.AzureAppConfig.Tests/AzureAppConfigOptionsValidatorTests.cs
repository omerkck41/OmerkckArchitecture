using Azure.Data.AppConfiguration;
using AwesomeAssertions;
using Kck.FeatureFlags.AzureAppConfig;
using Kck.FeatureFlags.AzureAppConfig.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Kck.FeatureFlags.AzureAppConfig.Tests;

public sealed class AzureAppConfigOptionsValidatorTests
{
    private readonly AzureAppConfigFeatureFlagOptionsValidator _sut = new();

    [Fact]
    public void Validate_ConnectionString_ReturnsSuccess()
    {
        var opts = new AzureAppConfigOptions { ConnectionString = "Endpoint=https://x.azconfig.io;Id=a;Secret=b" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_Endpoint_ReturnsSuccess()
    {
        var opts = new AzureAppConfigOptions { Endpoint = "https://mystore.azconfig.io" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_NeitherSet_ReturnsFail()
    {
        var opts = new AzureAppConfigOptions();
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString").And.Contain("Endpoint");
    }

    [Fact]
    public void Validate_BothSet_ReturnsFail()
    {
        var opts = new AzureAppConfigOptions
        {
            ConnectionString = "Endpoint=https://x.azconfig.io;Id=a;Secret=b",
            Endpoint = "https://mystore.azconfig.io"
        };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString").And.Contain("Endpoint");
    }

    [Fact]
    public void Validate_ZeroRefreshInterval_ReturnsFail()
    {
        var opts = new AzureAppConfigOptions
        {
            ConnectionString = "Endpoint=https://x.azconfig.io;Id=a;Secret=b",
            RefreshInterval = TimeSpan.Zero
        };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("RefreshInterval");
    }

    [Fact]
    public void Validate_NeitherSetButClientRegistered_ReturnsSuccess()
    {
        var sut = new AzureAppConfigFeatureFlagOptionsValidator(IsService(clientRegistered: true));

        sut.Validate(null, new AzureAppConfigOptions()).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_NeitherSetAndNoClientRegistered_ReturnsFail()
    {
        var sut = new AzureAppConfigFeatureFlagOptionsValidator(IsService(clientRegistered: false));

        var result = sut.Validate(null, new AzureAppConfigOptions());

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString").And.Contain("ConfigurationClient");
    }

    [Fact]
    public void Validate_BothSetWithClientRegistered_ReturnsFail()
    {
        var sut = new AzureAppConfigFeatureFlagOptionsValidator(IsService(clientRegistered: true));
        var opts = new AzureAppConfigOptions
        {
            ConnectionString = "Endpoint=https://x.azconfig.io;Id=a;Secret=b",
            Endpoint = "https://mystore.azconfig.io"
        };

        sut.Validate(null, opts).Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Validate_ZeroRefreshIntervalWithClientRegistered_ReturnsFail()
    {
        var sut = new AzureAppConfigFeatureFlagOptionsValidator(IsService(clientRegistered: true));

        var result = sut.Validate(null, new AzureAppConfigOptions { RefreshInterval = TimeSpan.Zero });

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("RefreshInterval");
    }

    private static IServiceProviderIsService IsService(bool clientRegistered)
    {
        var isService = Substitute.For<IServiceProviderIsService>();
        isService.IsService(typeof(ConfigurationClient)).Returns(clientRegistered);
        return isService;
    }
}
