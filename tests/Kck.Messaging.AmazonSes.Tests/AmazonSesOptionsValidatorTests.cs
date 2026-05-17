using FluentAssertions;
using Kck.Messaging.AmazonSes.DependencyInjection;
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
}
