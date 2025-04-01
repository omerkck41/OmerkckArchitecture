using Core.Application.Caching.Behaviors;
using Core.Application.Caching.Services;
using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application.Caching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration, Action<CacheSettings> configureSettings)
    {
        if (services == null)
            throw new CustomArgumentException(nameof(services));

        if (configureSettings == null)
            throw new CustomArgumentException(nameof(configureSettings));

        CacheSettings settings = new CacheSettings();
        configureSettings(settings);

        services.AddSingleton(settings);

        // Cache servislerini ekle
        if (settings.Provider == CacheProvider.InMemory)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }
        else if (settings.Provider == CacheProvider.Distributed)
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICacheService, DistributedCacheService>();

            // Distributed cache için Redis kullanılacaksa:
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = configuration.GetConnectionString("InstanceName");
            });
        }


        // MediatR pipeline'ına cache davranışlarını ekle
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheRemovingBehavior<,>));

        return services;
    }
}