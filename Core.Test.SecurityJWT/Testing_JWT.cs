using Core.Security.Entities;
using Core.Security.JWT;
using Microsoft.Extensions.Options;
using System.Security.Claims;


namespace Core.Test.SecurityJWT;


public class JwtHelperFullTests
{
    private readonly JwtHelper<int, string, int> _jwtHelper;
    private readonly InMemoryTokenBlacklistManager<int> _blacklistManager;
    private readonly TokenOptions _tokenOptions;

    const string LongSecretKey = "ThisIsAReallyLongSecretKeyThatIsAtLeast64CharactersLongToSatisfyHMACSHA512Requirements";

    public JwtHelperFullTests()
    {
        _tokenOptions = new TokenOptions("TestAudience", "TestIssuer", 2, LongSecretKey, 7);
        _blacklistManager = new InMemoryTokenBlacklistManager<int>();
        var options = Options.Create(_tokenOptions);
        _jwtHelper = new JwtHelper<int, string, int>(options, _blacklistManager);
    }

    [Fact]
    public void CreateToken_ValidToken_CorrectClaims()
    {
        // Arrange: Örnek bir kullanıcı ve operasyon claim'leri oluştur.
        var user = new User<int>
        {
            Id = 100,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        var operationClaims = new List<OperationClaim<string>>
        {
            new OperationClaim<string> { Name = "Admin" }
        };

        // Custom claim ekleyerek token oluştur.
        var accessToken = _jwtHelper.CreateToken(user, operationClaims, new Dictionary<string, string> { { "CustomClaim", "CustomValue" } });

        // Assert: Token oluşturulmuş, boş değil, süresi gelecekte ve doğrulama başarılı.
        Assert.NotNull(accessToken);
        Assert.False(string.IsNullOrEmpty(accessToken.Token));
        Assert.True(accessToken.ExpirationDate > DateTime.UtcNow);
        Assert.True(_jwtHelper.ValidateToken(accessToken.Token));

        // Token içerisinden claim'leri oku.
        var claims = _jwtHelper.GetClaimsFromToken(accessToken.Token).ToList();

        var nameIdentifierClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal(user.Id.ToString(), nameIdentifierClaim.Value);

        var emailClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(user.Email, emailClaim.Value);

        var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        Assert.NotNull(nameClaim);
        Assert.Equal($"{user.FirstName} {user.LastName}", nameClaim.Value);

        var roleClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);

        var customClaim = claims.FirstOrDefault(c => c.Type == "CustomClaim");
        Assert.NotNull(customClaim);
        Assert.Equal("CustomValue", customClaim.Value);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange: Yanlış formatta bir token.
        var invalidToken = "thisIsAnInvalidToken";

        // Act & Assert
        Assert.False(_jwtHelper.ValidateToken(invalidToken));
    }

    [Fact]
    public void RevokeToken_TokenIsInvalidated()
    {
        // Arrange: Örnek kullanıcı ve token oluştur.
        var user = new User<int>
        {
            Id = 200,
            Email = "revoke@example.com",
            FirstName = "Revoke",
            LastName = "Test"
        };
        var operationClaims = new List<OperationClaim<string>> { new OperationClaim<string> { Name = "User" } };

        var accessToken = _jwtHelper.CreateToken(user, operationClaims);
        Assert.True(_jwtHelper.ValidateToken(accessToken.Token));

        // Act: Token iptal ediliyor.
        _jwtHelper.RevokeToken(accessToken.Token, user.Id);

        // Assert: İptal sonrası token doğrulama başarısız olmalı.
        Assert.False(_jwtHelper.ValidateToken(accessToken.Token));
    }

    [Fact]
    public void GetUserIdFromToken_ReturnsCorrectUserId()
    {
        // Arrange: Kullanıcı oluştur ve token üret.
        var user = new User<int>
        {
            Id = 300,
            Email = "userid@example.com",
            FirstName = "Id",
            LastName = "Test"
        };
        var operationClaims = new List<OperationClaim<string>> { new OperationClaim<string> { Name = "Member" } };

        var accessToken = _jwtHelper.CreateToken(user, operationClaims);

        // Act: Token'dan kullanıcı ID'si çek.
        int extractedUserId = _jwtHelper.GetUserIdFromToken(accessToken.Token);

        // Assert
        Assert.Equal(user.Id, extractedUserId);
    }

    [Fact]
    public void GetExpirationDateFromToken_ReturnsCorrectExpiration()
    {
        // Arrange: Token üret ve beklenen son kullanma tarihini al.
        var user = new User<int>
        {
            Id = 400,
            Email = "expiration@example.com",
            FirstName = "Expire",
            LastName = "Test"
        };
        var operationClaims = new List<OperationClaim<string>> { new OperationClaim<string> { Name = "Member" } };

        var accessToken = _jwtHelper.CreateToken(user, operationClaims);
        DateTime expectedExpiration = accessToken.ExpirationDate;

        // Act: Token'dan son kullanma tarihi çek.
        DateTime expirationFromToken = _jwtHelper.GetExpirationDateFromToken(accessToken.Token);

        // Assert: Küçük zaman farklarına izin veriyoruz.
        Assert.InRange((expirationFromToken - expectedExpiration).TotalSeconds, -5, 5);
    }

    [Fact]
    public void CreateRefreshToken_CreatesValidRefreshToken()
    {
        // Arrange: Örnek kullanıcı oluştur ve IP adresi belirle.
        var user = new User<int>
        {
            Id = 500,
            Email = "refresh@example.com",
            FirstName = "Refresh",
            LastName = "Test"
        };
        string ipAddress = "127.0.0.1";

        // Act: Refresh token oluştur.
        var refreshToken = _jwtHelper.CreateRefreshToken(user, ipAddress);

        // Assert
        Assert.NotNull(refreshToken);
        Assert.Equal(user.Id, refreshToken.UserId);
        Assert.False(string.IsNullOrEmpty(refreshToken.Token));
        Assert.True(refreshToken.ExpirationDate > DateTime.UtcNow);
        Assert.Equal(ipAddress, refreshToken.CreatedByIp);
    }

    [Fact]
    public void RefreshToken_Revoke_SetsRevokedProperties()
    {
        // Arrange: Refresh token oluştur.
        var user = new User<int>
        {
            Id = 600,
            Email = "revokerefresh@example.com",
            FirstName = "Revoke",
            LastName = "Refresh"
        };
        string ipAddress = "192.168.1.1";
        var refreshToken = _jwtHelper.CreateRefreshToken(user, ipAddress);

        // Act: Refresh token'ı iptal et.
        refreshToken.Revoke("192.168.1.100", "Test revoke", "newTokenValue");

        // Assert: İptal bilgileri set edilmiş olmalı.
        Assert.NotNull(refreshToken.RevokedDate);
        Assert.Equal("192.168.1.100", refreshToken.RevokedByIp);
        Assert.Equal("newTokenValue", refreshToken.ReplacedByToken);
        Assert.Equal("Test revoke", refreshToken.ReasonRevoked);
    }

    [Fact]
    public void ExpiredToken_ShouldBeInvalid()
    {
        const string LongSecretKey = "ThisIsAReallyLongSecretKeyThatIsAtLeast64CharactersLongToSatisfyHMACSHA512Requirements";
        var expiredTokenOptions = new TokenOptions("TestAudience", "TestIssuer", -1, LongSecretKey, 7);
        var expiredOptions = Options.Create(expiredTokenOptions);
        var jwtHelperForExpired = new JwtHelper<int, string, int>(expiredOptions, new InMemoryTokenBlacklistManager<int>());

        var user = new User<int>
        {
            Id = 700,
            Email = "expired@example.com",
            FirstName = "Expired",
            LastName = "Test"
        };
        var operationClaims = new List<OperationClaim<string>> { new OperationClaim<string> { Name = "Member" } };

        var accessToken = jwtHelperForExpired.CreateToken(user, operationClaims);

        bool isValid = jwtHelperForExpired.ValidateToken(accessToken.Token);

        Assert.False(isValid);
    }
}
