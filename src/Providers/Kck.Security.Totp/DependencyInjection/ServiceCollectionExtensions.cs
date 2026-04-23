using Kck.Security.Abstractions.Mfa;
using Kck.Security.Totp;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecurityTotpServiceCollectionExtensions
{
    /// <summary>
    /// Registers TOTP MFA services including replay protection.
    /// Requires a singleton IMemoryCache (registered via AddMemoryCache).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional TOTP options configuration.</param>
    public static IServiceCollection AddKckTotp(
        this IServiceCollection services,
        Action<TotpOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<TotpOptions>(_ => { });

        services.AddMemoryCache();
        services.TryAddSingleton<IMfaProvider, TotpMfaProvider>();
        return services;
    }
}
