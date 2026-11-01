using Amazon.SimpleEmailV2;
using Kck.Messaging.Abstractions;
using Kck.Messaging.AmazonSes;
using Kck.Messaging.AmazonSes.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingAmazonSesServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Amazon SES <see cref="IEmailProvider"/> and validates <see cref="AmazonSesOptions"/> on start.
    /// When the container has an <see cref="IAmazonSimpleEmailServiceV2"/> registered, the provider uses that client
    /// (register your own client to control credentials or the endpoint) and <c>Region</c> may stay empty; otherwise
    /// it builds one from <see cref="AmazonSesOptions"/>.
    /// </summary>
    public static IServiceCollection AddKckMessagingAmazonSes(
        this IServiceCollection services,
        Action<AmazonSesOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AmazonSesOptions>, AmazonSesOptionsValidator>();
        services.AddOptions<AmazonSesOptions>().ValidateOnStart();
        services.TryAddSingleton<IEmailProvider>(sp =>
            sp.GetService<IAmazonSimpleEmailServiceV2>() is { } client
                ? new AmazonSesEmailProvider(client)
                : new AmazonSesEmailProvider(sp.GetRequiredService<IOptionsMonitor<AmazonSesOptions>>()));
        return services;
    }
}
