using Core.Security.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security.JWT;

public class JwtHelper<TUserId, TOperationClaimId, TRefreshTokenId> : ITokenHelper<TUserId, TOperationClaimId, TRefreshTokenId>
{
    private readonly TokenOptions _tokenOptions;
    private readonly IRefreshTokenRepository<TRefreshTokenId, TUserId> _refreshTokenRepository;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly SigningCredentials _signingCredentials;

    /// <summary>
    /// JwtHelper sınıfını başlatır ve gerekli bağımlılıkları yükler.
    /// </summary>
    /// <param name="tokenOptions">JWT token yapılandırma seçenekleri.</param>
    /// <param name="tokenBlacklist">Token kara liste kontrolü için kullanılacak sınıf.</param>
    /// <param name="refreshTokenRepository">Refresh token yönetimi için kullanılacak repository.</param>
    /// <exception cref="InvalidOperationException">Token yapılandırma seçenekleri yüklenemezse fırlatılır.</exception>
    public JwtHelper(IOptions<TokenOptions> tokenOptions, IRefreshTokenRepository<TRefreshTokenId, TUserId> refreshTokenRepository)
    {
        _tokenOptions = tokenOptions.Value ?? throw new InvalidOperationException("Token options are not configured.");

        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));
        _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha512Signature);
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    /// Kullanıcı ve operasyon yetkilerine göre yeni bir access token oluşturur.
    /// </summary>
    /// <param name="user">Token oluşturulacak kullanıcı.</param>
    /// <param name="operationClaims">Kullanıcının operasyon yetkileri.</param>
    /// <param name="customClaims">Ekstra claim'ler (isteğe bağlı).</param>
    /// <returns>Oluşturulan access token.</returns>
    public async Task<AccessToken> CreateTokenAsync(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims, IDictionary<string, string> customClaims = null)
    {
        var expirationDate = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration);
        var claims = SetClaims(user, operationClaims);

        // Custom claim'leri ekle
        if (customClaims != null)
        {
            foreach (var claim in customClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
        }

        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            claims: claims,
            expires: expirationDate,
            signingCredentials: _signingCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return await Task.FromResult(new AccessToken(token, expirationDate));
    }

    /// <summary>
    /// Kullanıcı için yeni bir refresh token oluşturur.
    /// </summary>
    /// <param name="user">Refresh token oluşturulacak kullanıcı.</param>
    /// <param name="ipAddress">IP adresi.</param>
    /// <returns>Oluşturulan refresh token.</returns>
    public async Task<RefreshToken<TRefreshTokenId, TUserId>> CreateRefreshTokenAsync(User<TUserId> user, string ipAddress)
    {
        var refreshToken = new RefreshToken<TRefreshTokenId, TUserId>(
            id: default!,
            userId: user.Id,
            token: GenerateRefreshToken(),
            expirationDate: DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenTTL),
            createdByIp: ipAddress
        );

        return await Task.FromResult(refreshToken);
    }

    /// <summary>
    /// Mevcut bir refresh token kullanarak yeni bir access token oluşturur.
    /// </summary>
    /// <param name="user">Token yenilenecek kullanıcı.</param>
    /// <param name="operationClaims">Kullanıcının operasyon yetkileri.</param>
    /// <param name="ipAddress">IP adresi.</param>
    /// <returns>Yenilenen access token.</returns>
    public async Task<AccessToken> RefreshTokenAsync(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims, string ipAddress)
    {
        // Eski refresh token'ı iptal et
        var oldRefreshToken = await _refreshTokenRepository.GetCurrentRefreshTokenAsync(user.Id);
        if (oldRefreshToken != null)
        {
            await _refreshTokenRepository.RevokeRefreshTokenAsync(oldRefreshToken.Id, ipAddress, "Replaced by new token");
        }

        // Yeni access token oluştur
        var newAccessToken = await CreateTokenAsync(user, operationClaims);

        // Yeni refresh token oluştur
        var newRefreshToken = await CreateRefreshTokenAsync(user, ipAddress);

        // Yeni refresh token'ı veritabanına kaydet
        await _refreshTokenRepository.AddRefreshTokenAsync(newRefreshToken);

        return newAccessToken;
    }

    /// <summary>
    /// Belirtilen bir token'ın geçerliliğini kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    /// <returns>Token geçerliyse true, aksi halde false.</returns>
    public async Task<bool> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _tokenOptions.Issuer,
            ValidAudience = _tokenOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey))
        };

        try
        {
            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch (SecurityTokenExpiredException)
        {
            throw new SecurityTokenException("Token has expired.");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            throw new SecurityTokenException("Invalid token signature.");
        }
        catch
        {
            throw new SecurityTokenException("Invalid token.");
        }
    }

    /// <summary>
    /// Belirtilen bir token'dan claim bilgilerini alır.
    /// </summary>
    /// <param name="token">Claim'leri alınacak token.</param>
    /// <returns>Token'daki claim bilgileri.</returns>
    public async Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        return await Task.FromResult(jwtToken.Claims);
    }

    /// <summary>
    /// Token içindeki kullanıcı ID'sini alır.
    /// </summary>
    /// <param name="token">Kullanıcı ID'si alınacak token.</param>
    /// <returns>Kullanıcı ID'si.</returns>
    public async Task<TUserId> GetUserIdFromTokenAsync(string token)
    {
        var claims = await GetClaimsFromTokenAsync(token);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new SecurityTokenException("User ID claim not found in token.");
        }
        return (TUserId)Convert.ChangeType(userIdClaim.Value, typeof(TUserId));
    }

    /// <summary>
    /// Token'ın geçerlilik tarihini alır.
    /// </summary>
    /// <param name="token">Geçerlilik tarihi alınacak token.</param>
    /// <returns>Token'ın geçerlilik bitiş tarihi.</returns>
    public async Task<DateTime> GetExpirationDateFromTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        return await Task.FromResult(jwtToken.ValidTo);
    }


    private List<Claim> SetClaims(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id!.ToString()!),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        claims.AddRange(operationClaims.Select(c => new Claim(ClaimTypes.Role, c.Name)));
        return claims;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}