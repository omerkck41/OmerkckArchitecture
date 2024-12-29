using Core.Application.Caching.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Caching;

public static class CacheExtensions
{
    public static IServiceCollection AddCacheServices(this IServiceCollection services, CacheSettings settings)
    {
        if (settings.Provider == "InMemory")
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        else if (settings.Provider == "Distributed")
            services.AddSingleton<ICacheService, DistributedCacheService>();

        services.AddSingleton(settings);
        return services;
    }
}
