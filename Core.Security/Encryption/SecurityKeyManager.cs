using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Core.Security.Encryption;

public class SecurityKeyManager
{
    private readonly IConfiguration _configuration;

    public SecurityKeyManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SecurityKey GetSecurityKey()
    {
        var key = _configuration["TokenOptions:SecurityKey"] ?? "omerkck";
        return SecurityKeyHelper.CreateSecurityKey(key);
    }
}