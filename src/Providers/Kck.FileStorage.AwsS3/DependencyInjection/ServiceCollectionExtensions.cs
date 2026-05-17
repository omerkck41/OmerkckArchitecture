using Kck.FileStorage.Abstractions;
using Kck.FileStorage.AwsS3;
using Kck.FileStorage.AwsS3.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extension methods for registering Amazon S3 file storage.</summary>
public static class KckFileStorageAwsS3ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AwsS3StorageService"/> as the <see cref="IFileStorageService"/>.
    /// Omit <c>AccessKey</c> / <c>SecretKey</c> to use IAM role or environment credentials.
    /// </summary>
    public static IServiceCollection AddKckFileStorageAwsS3(
        this IServiceCollection services,
        Action<AwsS3Options> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AwsS3Options>, AwsS3OptionsValidator>();
        services.AddOptions<AwsS3Options>().ValidateOnStart();
        services.TryAddSingleton<IFileStorageService, AwsS3StorageService>();
        return services;
    }
}
