using Kck.Security.Abstractions.Token;
using Kck.Security.Jwt;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecurityJwtServiceCollectionExtensions
{
    /// <summary>Adds JWT token service with RS256 signing.</summary>
    public static IServiceCollection AddKckJwt(
        this IServiceCollection services,
        Action<JwtOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<ITokenService, JwtTokenService>();
        return services;
    }
}
