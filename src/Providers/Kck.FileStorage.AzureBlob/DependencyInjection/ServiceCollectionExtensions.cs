using Kck.FileStorage.Abstractions;
using Kck.FileStorage.AzureBlob;
using Kck.FileStorage.AzureBlob.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extension methods for registering Azure Blob Storage file storage.</summary>
public static class KckFileStorageAzureBlobServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AzureBlobStorageService"/> as the <see cref="IFileStorageService"/>.
    /// Provide either <c>ConnectionString</c> or <c>AccountName</c> (Managed Identity) in the options.
    /// </summary>
    public static IServiceCollection AddKckFileStorageAzureBlob(
        this IServiceCollection services,
        Action<AzureBlobOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AzureBlobOptions>, AzureBlobOptionsValidator>();
        services.AddOptions<AzureBlobOptions>().ValidateOnStart();
        services.TryAddSingleton<IFileStorageService, AzureBlobStorageService>();
        return services;
    }
}
