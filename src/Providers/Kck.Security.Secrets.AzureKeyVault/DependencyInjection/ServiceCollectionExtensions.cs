using Kck.Security.Abstractions.Secrets;
using Kck.Security.Secrets.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecuritySecretsAzureKeyVaultServiceCollectionExtensions
{
    public static IServiceCollection AddKckAzureKeyVault(
        this IServiceCollection services,
        Action<AzureKeyVaultOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<ISecretsManager, AzureKeyVaultSecretsManager>();
        return services;
    }
}
