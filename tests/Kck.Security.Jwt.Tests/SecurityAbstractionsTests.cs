using AwesomeAssertions;
using Kck.Security.Abstractions.Entities;
using Kck.Security.Abstractions.Mfa;
using Kck.Security.Abstractions.Token;
using Xunit;

namespace Kck.Security.Jwt.Tests;

public sealed class SecurityAbstractionsTests
{
    [Fact]
    public void RefreshToken_NewActiveToken_IsActive()
    {
        var token = new RefreshToken<Guid, int> { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };

        token.IsExpired.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_PastExpiry_IsExpiredAndInactive()
    {
        var token = new RefreshToken<Guid, int> { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) };

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_DefaultExpiry_IsExpired()
    {
        new RefreshToken<Guid, int>().IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Revoke_WithReplacement_SetsAllRevocationFields()
    {
        var token = new RefreshToken<Guid, int> { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        var before = DateTime.UtcNow;

        token.Revoke("10.0.0.1", "rotated", "new-token");

        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
        token.RevokedAt.Should().NotBeNull().And.BeOnOrAfter(before);
        token.RevokedByIp.Should().Be("10.0.0.1");
        token.RevokeReason.Should().Be("rotated");
        token.ReplacedByToken.Should().Be("new-token");
    }

    [Fact]
    public void Revoke_WithoutReplacement_LeavesReplacedByNull()
    {
        var token = new RefreshToken<Guid, int> { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };

        token.Revoke("ip", "logout");

        token.ReplacedByToken.Should().BeNull();
    }

    [Fact]
    public void TokenRequest_Defaults_HaveEmptyRolesAndClaims()
    {
        var request = new TokenRequest { UserId = "1", Email = "a@b.c" };

        request.Name.Should().BeNull();
        request.Roles.Should().BeEmpty();
        request.CustomClaims.Should().BeEmpty();
    }

    [Fact]
    public void TokenValidationResult_Defaults_AreInvalidAndEmpty()
    {
        var result = new TokenValidationResult();

        result.IsValid.Should().BeFalse();
        result.UserId.Should().BeNull();
        result.Email.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.Roles.Should().BeEmpty();
        result.Claims.Should().BeEmpty();
    }

    [Fact]
    public void TokenResultAndMfaSetupResult_RecordEquality_UsesValues()
    {
        var at = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        new TokenResult { AccessToken = "t", ExpiresAt = at }
            .Should().Be(new TokenResult { AccessToken = "t", ExpiresAt = at });
        new MfaSetupResult { SecretKey = "k", ProvisioningUri = "otpauth://x" }
            .Should().NotBe(new MfaSetupResult { SecretKey = "k2", ProvisioningUri = "otpauth://x" });
    }
}
