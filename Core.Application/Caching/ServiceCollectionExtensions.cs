using Core.Application.Caching.Behaviors;
using Core.Application.Caching.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Caching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services, Action<CacheSettings> configureSettings)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureSettings == null)
            throw new ArgumentNullException(nameof(configureSettings));

        var settings = new CacheSettings();
        configureSettings(settings);

        // Cache servislerini ekle
        if (settings.Provider == CacheProvider.InMemory)
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        else if (settings.Provider == CacheProvider.Distributed)
            services.AddSingleton<ICacheService, DistributedCacheService>();

        services.AddSingleton(settings);

        // MediatR pipeline'ına cache davranışlarını ekle
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheRemovingBehavior<,>));

        return services;
    }
}