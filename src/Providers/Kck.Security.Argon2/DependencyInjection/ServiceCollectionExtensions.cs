using Kck.Security.Abstractions.Hashing;
using Kck.Security.Argon2;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecurityArgon2ServiceCollectionExtensions
{
    /// <summary>Adds Argon2id password hashing service.</summary>
    public static IServiceCollection AddKckArgon2(
        this IServiceCollection services,
        Action<Argon2Options>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<Argon2Options>(_ => { });

        services.TryAddSingleton<IHashingService, Argon2HashingService>();
        return services;
    }
}
