using Kck.FileStorage.Abstractions;
using Kck.FileStorage.FluentFtp;
using Kck.FileStorage.FluentFtp.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckFileStorageFluentFtpServiceCollectionExtensions
{
    public static IServiceCollection AddKckFileStorageFluentFtp(
        this IServiceCollection services,
        Action<FluentFtpOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<FluentFtpOptions>, FluentFtpOptionsValidator>();
        services.AddOptions<FluentFtpOptions>().ValidateOnStart();
        services.TryAddSingleton<FtpConnectionPool>();
        services.AddTransient<IFtpService, FluentFtpService>();
        services.AddTransient<IFileStorageService>(sp => sp.GetRequiredService<IFtpService>());
        return services;
    }
}
