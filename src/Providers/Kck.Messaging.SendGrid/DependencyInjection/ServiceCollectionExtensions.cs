using Kck.Messaging.Abstractions;
using Kck.Messaging.SendGrid;
using Kck.Messaging.SendGrid.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SendGrid;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingSendGridServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SendGrid <see cref="IEmailProvider"/> and validates <see cref="SendGridOptions"/> on start.
    /// When the container has an <see cref="ISendGridClient"/> registered, the provider uses that client (register
    /// your own client for custom <see cref="SendGridClientOptions"/>) and <c>ApiKey</c> may stay empty; otherwise it
    /// builds one from <see cref="SendGridOptions.ApiKey"/>.
    /// </summary>
    public static IServiceCollection AddKckMessagingSendGrid(
        this IServiceCollection services,
        Action<SendGridOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<SendGridOptions>, SendGridOptionsValidator>();
        services.AddOptions<SendGridOptions>().ValidateOnStart();
        services.TryAddSingleton<IEmailProvider>(sp =>
            sp.GetService<ISendGridClient>() is { } client
                ? new SendGridEmailProvider(client)
                : new SendGridEmailProvider(sp.GetRequiredService<IOptionsMonitor<SendGridOptions>>()));
        return services;
    }
}
