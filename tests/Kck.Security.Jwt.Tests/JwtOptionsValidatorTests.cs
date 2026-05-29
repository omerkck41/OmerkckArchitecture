using AwesomeAssertions;
using Kck.Security.Jwt.DependencyInjection;
using Xunit;

namespace Kck.Security.Jwt.Tests;

public sealed class JwtOptionsValidatorTests
{
    private readonly JwtOptionsValidator _sut = new();

    [Fact]
    public void Validate_ConfigurationSource_ValidBase64Key_ReturnsSuccess()
    {
        var opts = Valid(KeySource: RsaKeySource.Configuration, RsaKeyBase64: "dGVzdA==");
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_FileSource_ValidPath_ReturnsSuccess()
    {
        var opts = Valid(KeySource: RsaKeySource.File, RsaKeyPath: "/secrets/rsa.pem");
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyIssuer_ReturnsFail(string issuer)
    {
        var opts = Valid(); opts.Issuer = issuer;
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Issuer");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyAudience_ReturnsFail(string audience)
    {
        var opts = Valid(); opts.Audience = audience;
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Audience");
    }

    [Fact]
    public void Validate_ZeroExpiration_ReturnsFail()
    {
        var opts = Valid(); opts.AccessTokenExpiration = TimeSpan.Zero;
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("AccessTokenExpiration");
    }

    [Fact]
    public void Validate_ConfigurationSource_MissingBase64Key_ReturnsFail()
    {
        var opts = Valid(KeySource: RsaKeySource.Configuration, RsaKeyBase64: null);
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("RsaKeyBase64");
    }

    [Fact]
    public void Validate_FileSource_MissingPath_ReturnsFail()
    {
        var opts = Valid(KeySource: RsaKeySource.File, RsaKeyPath: null);
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("RsaKeyPath");
    }

    [Fact]
    public void Validate_ZeroRefreshTokenTtl_ReturnsFail()
    {
        var opts = Valid(); opts.RefreshTokenTtlDays = 0;
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("RefreshTokenTtlDays");
    }

    [Fact]
    public void Validate_RefreshTokenTtlOfOneDay_ReturnsSuccess()
    {
        // Lower boundary: 1 is the minimum valid value (< 1 fails, so 1 must pass).
        var opts = Valid(); opts.RefreshTokenTtlDays = 1;
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    private static JwtOptions Valid(
        RsaKeySource KeySource = RsaKeySource.Configuration,
        string? RsaKeyBase64 = "dGVzdA==",
        string? RsaKeyPath = null) => new()
    {
        Issuer = "https://myapp.example.com",
        Audience = "api://myapp",
        AccessTokenExpiration = TimeSpan.FromMinutes(15),
        RefreshTokenTtlDays = 7,
        KeySource = KeySource,
        RsaKeyBase64 = RsaKeyBase64,
        RsaKeyPath = RsaKeyPath
    };
}
