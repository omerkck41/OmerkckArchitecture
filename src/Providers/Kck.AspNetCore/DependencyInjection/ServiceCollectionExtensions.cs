using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckAspNetCoreServiceCollectionExtensions
{
    public static IServiceCollection AddKckAspNetCore(
        this IServiceCollection services,
        Action<KckAspNetCoreBuilder> configure)
    {
        var builder = new KckAspNetCoreBuilder(services);
        configure(builder);

        services.TryAddSingleton(builder);

        return services;
    }
}
