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
    private readonly Lazy<SymmetricSecurityKey> _securityKey;
    private readonly Lazy<SigningCredentials> _signingCredentials;
    private readonly ITokenBlacklistManager<TUserId> _tokenBlacklistManager;

    /// <summary>
    /// JwtHelper sınıfını başlatır ve gerekli bağımlılıkları yükler.
    /// </summary>
    /// <param name="tokenOptions">JWT token yapılandırma seçenekleri.</param>
    /// <param name="tokenBlacklist">Token kara liste kontrolü için kullanılacak sınıf.</param>
    /// <param name="refreshTokenRepository">Refresh token yönetimi için kullanılacak repository.</param>
    /// <exception cref="InvalidOperationException">Token yapılandırma seçenekleri yüklenemezse fırlatılır.</exception>
    public JwtHelper(IOptions<TokenOptions> tokenOptions, ITokenBlacklistManager<TUserId> tokenBlacklistManager)
    {
        _tokenOptions = tokenOptions.Value ?? throw new InvalidOperationException("Token options are not configured.");
        _tokenBlacklistManager = tokenBlacklistManager;

        if (string.IsNullOrWhiteSpace(_tokenOptions.SecurityKey))
            throw new InvalidOperationException("SecurityKey is not configured properly.");

        _securityKey = new Lazy<SymmetricSecurityKey>(() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey)));
        _signingCredentials = new Lazy<SigningCredentials>(() => new SigningCredentials(_securityKey.Value, SecurityAlgorithms.HmacSha512Signature));
    }

    /// <summary>
    /// Kullanıcı ve operasyon yetkilerine göre yeni bir access token oluşturur.
    /// </summary>
    /// <param name="user">Token oluşturulacak kullanıcı.</param>
    /// <param name="operationClaims">Kullanıcının operasyon yetkileri.</param>
    /// <param name="customClaims">Ekstra claim'ler (isteğe bağlı).</param>
    /// <returns>Oluşturulan access token.</returns>
    public AccessToken CreateToken(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims, IDictionary<string, string> customClaims = null)
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
            signingCredentials: _signingCredentials.Value
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new AccessToken(token, expirationDate);
    }

    /// <summary>
    /// Kullanıcı için yeni bir refresh token oluşturur.
    /// </summary>
    /// <param name="user">Refresh token oluşturulacak kullanıcı.</param>
    /// <param name="ipAddress">IP adresi.</param>
    /// <returns>Oluşturulan refresh token.</returns>
    public RefreshToken<TRefreshTokenId, TUserId> CreateRefreshToken(User<TUserId> user, string ipAddress)
    {
        var refreshToken = new RefreshToken<TRefreshTokenId, TUserId>(
            id: (TRefreshTokenId)default!,
            userId: user.Id,
            token: GenerateRefreshToken(),
            expirationDate: DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenTTL),
            createdByIp: ipAddress
        );

        return refreshToken;
    }

    /// <summary>
    /// Belirtilen bir token'ın geçerliliğini kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    /// <returns>Token geçerliyse true, aksi halde false.</returns>
    public bool ValidateToken(string token)
    {
        if (_tokenBlacklistManager.IsTokenRevoked(token))
            return false;

        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                ValidIssuer = _tokenOptions.Issuer,
                ValidAudience = _tokenOptions.Audience,
                IssuerSigningKey = _securityKey.Value,
                LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
                {
                    // Eğer tokenın bitiş tarihi (expires) varsa ve şu anki zaman ondan küçükse token geçerli kabul edilir.
                    return expires != null && expires > DateTime.UtcNow;
                }
            }, out _);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void RevokeToken(string token, TUserId userId, TimeSpan? expiration = null)
    {
        var revokeDuration = expiration ?? TimeSpan.FromMinutes(_tokenOptions.AccessTokenExpiration);
        _tokenBlacklistManager.RevokeToken(token, userId, revokeDuration);
    }

    /// <summary>
    /// Belirtilen bir token'dan claim bilgilerini alır.
    /// </summary>
    /// <param name="token">Claim'leri alınacak token.</param>
    /// <returns>Token'daki claim bilgileri.</returns>
    public IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
    }

    /// <summary>
    /// Token içindeki kullanıcı ID'sini alır.
    /// </summary>
    /// <param name="token">Kullanıcı ID'si alınacak token.</param>
    /// <returns>Kullanıcı ID'si.</returns>
    public TUserId GetUserIdFromToken(string token)
    {
        var claims = GetClaimsFromToken(token);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
            throw new SecurityTokenException("User ID claim not found in token.");

        return (TUserId)Convert.ChangeType(userIdClaim, typeof(TUserId));
    }

    /// <summary>
    /// Token'ın geçerlilik tarihini alır.
    /// </summary>
    /// <param name="token">Geçerlilik tarihi alınacak token.</param>
    /// <returns>Token'ın geçerlilik bitiş tarihi.</returns>
    public DateTime GetExpirationDateFromToken(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
    }


    private List<Claim> SetClaims(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Convert.ToString(user.Id) ?? string.Empty),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        claims.AddRange(operationClaims.Select(c => new Claim(ClaimTypes.Role, c.Name)));
        return claims;
    }
    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}