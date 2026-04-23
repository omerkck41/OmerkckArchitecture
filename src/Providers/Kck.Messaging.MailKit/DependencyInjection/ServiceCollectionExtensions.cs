using Kck.Messaging.Abstractions;
using Kck.Messaging.MailKit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingMailKitServiceCollectionExtensions
{
    public static IServiceCollection AddKckMessagingMailKit(
        this IServiceCollection services,
        Action<MailKitOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<SmtpConnectionPool>();
        services.TryAddSingleton<IEmailProvider, MailKitEmailProvider>();
        return services;
    }
}
