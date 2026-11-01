using Azure.Security.KeyVault.Secrets;
using Kck.Security.Abstractions.Secrets;
using Kck.Security.Secrets.AzureKeyVault;
using Kck.Security.Secrets.AzureKeyVault.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckSecuritySecretsAzureKeyVaultServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Azure Key Vault <see cref="ISecretsManager"/> and validates <see cref="AzureKeyVaultOptions"/> on
    /// start. When the container has a <see cref="SecretClient"/> registered, the manager uses that client (register
    /// your own client for another credential or client options) and <c>VaultUri</c> may stay empty; otherwise it
    /// builds one for <c>VaultUri</c> with <c>DefaultAzureCredential</c>.
    /// </summary>
    public static IServiceCollection AddKckAzureKeyVault(
        this IServiceCollection services,
        Action<AzureKeyVaultOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AzureKeyVaultOptions>, AzureKeyVaultOptionsValidator>();
        services.AddOptions<AzureKeyVaultOptions>().ValidateOnStart();
        services.TryAddSingleton<ISecretsManager>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<AzureKeyVaultOptions>>();
            var logger = sp.GetRequiredService<ILogger<AzureKeyVaultSecretsManager>>();
            return sp.GetService<SecretClient>() is { } client
                ? new AzureKeyVaultSecretsManager(options, logger, client)
                : new AzureKeyVaultSecretsManager(options, logger);
        });
        return services;
    }
}
