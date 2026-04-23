using Kck.Caching.Abstractions;
using Kck.Caching.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckCachingInMemoryServiceCollectionExtensions
{
    public static IServiceCollection AddKckCachingInMemory(
        this IServiceCollection services,
        Action<CacheOptions>? configure = null)
    {
        services.AddMemoryCache();

        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<CacheOptions>(_ => { });

        services.TryAddSingleton<ICacheService, InMemoryCacheService>();
        return services;
    }
}
