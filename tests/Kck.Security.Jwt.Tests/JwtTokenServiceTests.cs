using FluentAssertions;
using Kck.Security.Abstractions.Token;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Kck.Security.Jwt.Tests;

public class JwtTokenServiceTests : IDisposable
{
    private readonly ITokenBlacklistService _blacklist = Substitute.For<ITokenBlacklistService>();
    private readonly JwtTokenService _sut;
    private readonly JwtOptions _options;

    public JwtTokenServiceTests()
    {
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        var keyBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

        _options = new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpiration = TimeSpan.FromMinutes(15),
            KeySource = RsaKeySource.Configuration,
            RsaKeyBase64 = keyBase64
        };

        _sut = new JwtTokenService(
            new StaticOptionsMonitor<JwtOptions>(_options),
            NullLogger<JwtTokenService>.Instance,
            _blacklist);
    }

    public void Dispose()
    {
        _sut.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidSignature_ShouldNotCallBlacklist()
    {
        var fakeToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJmYWtlLWp0aSJ9.invalidsignature";

        var result = await _sut.ValidateTokenAsync(fakeToken);

        await _blacklist.DidNotReceive().IsRevokedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ShouldCheckBlacklist()
    {
        var request = new TokenRequest { UserId = "user-1", Email = "test@test.com" };
        var token = await _sut.CreateAccessTokenAsync(request);
        _blacklist.IsRevokedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeTrue();
        await _blacklist.Received(1).IsRevokedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateTokenAsync_RevokedToken_ShouldReturnInvalid()
    {
        var request = new TokenRequest { UserId = "user-1", Email = "test@test.com" };
        var token = await _sut.CreateAccessTokenAsync(request);
        _blacklist.IsRevokedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("revoked");
    }

    [Fact]
    public async Task CreateAccessTokenAsync_ShouldReturnValidToken()
    {
        var request = new TokenRequest
        {
            UserId = "user-1",
            Email = "test@test.com",
            Name = "Test User",
            Roles = ["Admin", "User"]
        };

        var result = await _sut.CreateAccessTokenAsync(request);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ValidateTokenAsync_ExpiredToken_ShouldReturnInvalid()
    {
        var shortOptions = new JwtOptions
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            AccessTokenExpiration = TimeSpan.FromMilliseconds(1),
            KeySource = _options.KeySource,
            RsaKeyBase64 = _options.RsaKeyBase64
        };
        var shortSut = new JwtTokenService(
            new StaticOptionsMonitor<JwtOptions>(shortOptions),
            NullLogger<JwtTokenService>.Instance,
            _blacklist);

        var token = await shortSut.CreateAccessTokenAsync(new TokenRequest
        {
            UserId = "user-1", Email = "test@test.com"
        });

        await Task.Delay(50);

        var result = await shortSut.ValidateTokenAsync(token.AccessToken);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_ShouldReturnBase64String()
    {
        var result = await _sut.CreateRefreshTokenAsync();

        result.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(result);
        bytes.Length.Should().Be(32);
    }
}
