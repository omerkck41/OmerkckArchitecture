using AwesomeAssertions;
using Kck.FeatureFlags.AzureAppConfig;
using Kck.FeatureFlags.AzureAppConfig.DependencyInjection;
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
}
