using Kck.Messaging.Abstractions;
using Kck.Messaging.SendGrid;
using Kck.Messaging.SendGrid.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingSendGridServiceCollectionExtensions
{
    public static IServiceCollection AddKckMessagingSendGrid(
        this IServiceCollection services,
        Action<SendGridOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<SendGridOptions>, SendGridOptionsValidator>();
        services.AddOptions<SendGridOptions>().ValidateOnStart();
        services.TryAddSingleton<IEmailProvider, SendGridEmailProvider>();
        return services;
    }
}
