using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Core.Security.Encryption;

public static class SecurityKeyHelper
{
    public static SymmetricSecurityKey CreateSecurityKey(string securityKey)
    {
        if (string.IsNullOrEmpty(securityKey))
            throw new CustomException(nameof(securityKey));

        return new SymmetricSecurityKey(Convert.FromBase64String(securityKey));
    }
}
