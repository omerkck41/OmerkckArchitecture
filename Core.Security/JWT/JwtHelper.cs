using Core.Security.Encryption;
using Core.Security.Entities;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Core.Security.JWT;

public class JwtHelper<TUserId, TOperationClaimId, TRefreshTokenId> : ITokenHelper<TUserId, TOperationClaimId, TRefreshTokenId>
{
    private readonly TokenOptions _tokenOptions;
    private DateTime _accessTokenExpiration;

    public JwtHelper(IConfiguration configuration)
    {
        _tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>()
                        ?? throw new InvalidOperationException("Token options are not configured.");

        _tokenOptions.SecurityKey ??= "DefaultSecurityKey";
    }

    public async Task<AccessToken> CreateTokenAsync(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims)
    {
        _accessTokenExpiration = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration);
        var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey!);
        var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);

        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            expires: _accessTokenExpiration,
            claims: SetClaims(user, operationClaims),
            signingCredentials: signingCredentials
        );

        return await Task.FromResult(new AccessToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwt),
            ExpirationDate = _accessTokenExpiration
        });
    }

    private string randomRefreshToken()
    {
        byte[] numberByte = new byte[32];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(numberByte);
        return Convert.ToBase64String(numberByte);
    }
    public async Task<RefreshToken<TRefreshTokenId, TUserId>> CreateRefreshTokenAsync(User<TUserId> user, string ipAddress)
    {
        return await Task.FromResult(new RefreshToken<TRefreshTokenId, TUserId>
        {
            Token = randomRefreshToken(),
            ExpirationDate = DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenTTL),
            RevokedDate = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserId = user.Id
        });
    }

    private IEnumerable<Claim> SetClaims(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims)
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
}
