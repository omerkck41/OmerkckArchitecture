using Kck.Messaging.Abstractions;
using Kck.Messaging.AmazonSes;
using Kck.Messaging.AmazonSes.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingAmazonSesServiceCollectionExtensions
{
    public static IServiceCollection AddKckMessagingAmazonSes(
        this IServiceCollection services,
        Action<AmazonSesOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AmazonSesOptions>, AmazonSesOptionsValidator>();
        services.AddOptions<AmazonSesOptions>().ValidateOnStart();
        services.TryAddSingleton<IEmailProvider, AmazonSesEmailProvider>();
        return services;
    }
}
