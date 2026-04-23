using Kck.Security.Abstractions.Secrets;
using Kck.Security.Secrets.UserSecrets;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecuritySecretsUserSecretsServiceCollectionExtensions
{
    /// <summary>
    /// Adds IConfiguration-based secrets manager.
    /// Reads from environment variables, User Secrets, appsettings.json, etc.
    /// </summary>
    public static IServiceCollection AddKckUserSecrets(this IServiceCollection services)
    {
        services.TryAddSingleton<ISecretsManager, ConfigurationSecretsManager>();
        return services;
    }
}
