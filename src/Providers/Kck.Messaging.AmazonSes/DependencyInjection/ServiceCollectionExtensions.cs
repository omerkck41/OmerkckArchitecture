using Kck.Messaging.Abstractions;
using Kck.Messaging.AmazonSes;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingAmazonSesServiceCollectionExtensions
{
    public static IServiceCollection AddKckMessagingAmazonSes(
        this IServiceCollection services,
        Action<AmazonSesOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IEmailProvider, AmazonSesEmailProvider>();
        return services;
    }
}
