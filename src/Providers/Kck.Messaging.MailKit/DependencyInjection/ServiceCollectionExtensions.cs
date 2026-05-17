using Kck.Messaging.Abstractions;
using Kck.Messaging.MailKit;
using Kck.Messaging.MailKit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckMessagingMailKitServiceCollectionExtensions
{
    public static IServiceCollection AddKckMessagingMailKit(
        this IServiceCollection services,
        Action<MailKitOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<MailKitOptions>, MailKitOptionsValidator>();
        services.AddOptions<MailKitOptions>().ValidateOnStart();
        services.TryAddSingleton<SmtpConnectionPool>();
        services.TryAddSingleton<IEmailProvider, MailKitEmailProvider>();
        return services;
    }
}
