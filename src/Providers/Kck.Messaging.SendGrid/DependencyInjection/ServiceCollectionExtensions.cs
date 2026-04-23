using Kck.Messaging.Abstractions;
using Kck.Messaging.SendGrid;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingSendGridServiceCollectionExtensions
{
    public static IServiceCollection AddKckMessagingSendGrid(
        this IServiceCollection services,
        Action<SendGridOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IEmailProvider, SendGridEmailProvider>();
        return services;
    }
}
