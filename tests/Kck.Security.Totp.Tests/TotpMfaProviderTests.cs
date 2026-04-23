using FluentAssertions;
using Kck.Security.Totp;
using Kck.Testing;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Kck.Security.Totp.Tests;

public class TotpMfaProviderTests : IDisposable
{
    private readonly TotpMfaProvider _sut;
    private readonly TotpOptions _totpOptions = new();
    private readonly MemoryCache _replayCache = new(new MemoryCacheOptions());

    public TotpMfaProviderTests()
    {
        _sut = new TotpMfaProvider(new StaticOptionsMonitor<TotpOptions>(_totpOptions), _replayCache);
    }

    public void Dispose()
    {
        _replayCache.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ValidateCodeAsync_SameCodeUsedTwice_ShouldRejectSecondAttempt()
    {
        // Arrange
        var setup = await _sut.GenerateSetupAsync("testuser", "TestApp");
        var totp = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(setup.SecretKey),
            step: _totpOptions.StepSeconds, totpSize: _totpOptions.CodeLength, mode: OtpNet.OtpHashMode.Sha256);
        var code = totp.ComputeTotp();

        // Act
        var firstResult = await _sut.ValidateCodeAsync(setup.SecretKey, code);
        var secondResult = await _sut.ValidateCodeAsync(setup.SecretKey, code);

        // Assert
        firstResult.Should().BeTrue();
        secondResult.Should().BeFalse("ayni kod ikinci kez kullanilamaz");
    }

    [Fact]
    public async Task ValidateCodeAsync_ValidCode_ShouldReturnTrue()
    {
        // Arrange
        var setup = await _sut.GenerateSetupAsync("testuser", "TestApp");
        var totp = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(setup.SecretKey),
            step: _totpOptions.StepSeconds, totpSize: _totpOptions.CodeLength, mode: OtpNet.OtpHashMode.Sha256);
        var code = totp.ComputeTotp();

        // Act
        var result = await _sut.ValidateCodeAsync(setup.SecretKey, code);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCodeAsync_InvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var setup = await _sut.GenerateSetupAsync("testuser", "TestApp");

        // Act
        var result = await _sut.ValidateCodeAsync(setup.SecretKey, "000000");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateSetupAsync_ShouldReturnValidSecretAndUri()
    {
        // Act
        var result = await _sut.GenerateSetupAsync("user@test.com", "MyApp");

        // Assert
        result.SecretKey.Should().NotBeNullOrEmpty();
        result.ProvisioningUri.Should().Contain("otpauth://totp/");
        result.ProvisioningUri.Should().Contain("MyApp");
    }
}
