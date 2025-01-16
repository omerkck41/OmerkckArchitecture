using Core.Security.Encryption;
using Core.Security.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Security.JWT;

public class JwtTokenService<TId, TOperationClaimId>
{
    private readonly TokenOptions _tokenOptions;

    public JwtTokenService(TokenOptions tokenOptions)
    {
        _tokenOptions = tokenOptions;
    }

    public async Task<AccessToken> GenerateAccessTokenAsync(User<TId> user, IList<OperationClaim<TOperationClaimId>> claims)
    {
        var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey!);
        var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);

        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            expires: DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration),
            claims: SetClaims(user, claims),
            signingCredentials: signingCredentials
        );

        return await Task.FromResult(new AccessToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwt),
            ExpirationDate = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration)
        });
    }

    private IEnumerable<Claim> SetClaims(User<TId> user, IList<OperationClaim<TOperationClaimId>> operationClaims)
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