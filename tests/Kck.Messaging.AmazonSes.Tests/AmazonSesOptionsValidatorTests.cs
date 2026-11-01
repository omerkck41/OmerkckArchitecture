using Amazon.SimpleEmailV2;
using AwesomeAssertions;
using Kck.Messaging.AmazonSes.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Messaging.AmazonSes.Tests;

public sealed class AmazonSesOptionsValidatorTests
{
    private readonly AmazonSesOptionsValidator _sut = new();

    [Fact]
    public void Validate_ValidRegionNoCredentials_ReturnsSuccess()
    {
        var opts = new AmazonSesOptions { Region = "eu-central-1" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidRegionWithCredentials_ReturnsSuccess()
    {
        var opts = new AmazonSesOptions { Region = "eu-central-1", AccessKey = "AKIA", SecretKey = "secret" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyRegion_ReturnsFail(string region)
    {
        var opts = new AmazonSesOptions { Region = region };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Region");
    }

    [Fact]
    public void Validate_OnlyAccessKeySet_ReturnsFail()
    {
        var opts = new AmazonSesOptions { Region = "eu-central-1", AccessKey = "AKIA", SecretKey = null };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("SecretKey");
    }

    [Fact]
    public void Validate_OnlySecretKeySet_ReturnsFail()
    {
        var opts = new AmazonSesOptions { Region = "eu-central-1", AccessKey = null, SecretKey = "secret" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("AccessKey");
    }

    [Fact]
    public void Validate_EmptyRegionWithRegisteredClient_ReturnsSuccess()
    {
        var sut = new AmazonSesOptionsValidator(new FakeIsService(sesClientRegistered: true));

        sut.Validate(null, new AmazonSesOptions { Region = "" }).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyRegionWithoutRegisteredClient_ReturnsFail()
    {
        var sut = new AmazonSesOptionsValidator(new FakeIsService(sesClientRegistered: false));

        var result = sut.Validate(null, new AmazonSesOptions { Region = "" });

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Region").And.Contain("IAmazonSimpleEmailServiceV2");
    }

    [Fact]
    public void Validate_OnlyAccessKeySetWithRegisteredClient_ReturnsFail()
    {
        var sut = new AmazonSesOptionsValidator(new FakeIsService(sesClientRegistered: true));

        var result = sut.Validate(null, new AmazonSesOptions { Region = "", AccessKey = "AKIA" });

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("SecretKey");
    }

    /// <summary>Answers <see cref="IServiceProviderIsService"/> for the SES client type only.</summary>
    private sealed class FakeIsService(bool sesClientRegistered) : IServiceProviderIsService
    {
        public bool IsService(Type serviceType) =>
            serviceType == typeof(IAmazonSimpleEmailServiceV2) && sesClientRegistered;
    }
}
