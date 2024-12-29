using Core.Security.Encryption;
using Core.Security.JWT;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Core.Security.Validators;

public static class TokenValidator
{
    public static bool ValidateToken(string token, TokenOptions tokenOptions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey!);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = tokenOptions.Issuer,
                ValidAudience = tokenOptions.Audience,
                IssuerSigningKey = key
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}