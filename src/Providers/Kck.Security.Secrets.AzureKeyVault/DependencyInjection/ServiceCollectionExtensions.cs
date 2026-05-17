using Kck.Security.Abstractions.Secrets;
using Kck.Security.Secrets.AzureKeyVault;
using Kck.Security.Secrets.AzureKeyVault.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecuritySecretsAzureKeyVaultServiceCollectionExtensions
{
    public static IServiceCollection AddKckAzureKeyVault(
        this IServiceCollection services,
        Action<AzureKeyVaultOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AzureKeyVaultOptions>, AzureKeyVaultOptionsValidator>();
        services.AddOptions<AzureKeyVaultOptions>().ValidateOnStart();
        services.TryAddSingleton<ISecretsManager, AzureKeyVaultSecretsManager>();
        return services;
    }
}
