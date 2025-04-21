using Core.Localization.Abstract;
using Core.Localization.Behaviors;
using Core.Localization.Concrete;
using Core.Localization.Constants;
using Core.Localization.Models;
using Core.Localization.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Localization.Extensions;

/// <summary>
/// IServiceCollection için uzantı metotları
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Lokalizasyon servislerini ekler
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <param name="configuration">Yapılandırma</param>
    /// <returns>Servis koleksiyonu</returns>
    public static IServiceCollection AddCoreLocalization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LocalizationPipelineBehavior<,>));


        // Yapılandırmayı bağla
        services.Configure<LocalizationOptions>(
            configuration.GetSection(LocalizationConstants.ConfigurationSectionName));

        // CurrencyOptions’ı bağlayın
        services.Configure<CurrencyOptions>(
            configuration.GetSection($"{LocalizationConstants.ConfigurationSectionName}:Currency"));

        // DateTimeOptions’ı bağlayın
        services.Configure<DateTimeOptions>(
            configuration.GetSection($"{LocalizationConstants.ConfigurationSectionName}:DateTime"));

        // Önbellek servisi
        // Yerel bellek üzerinde cache için (test/dev ortamı):
        services.AddDistributedMemoryCache();

        services.AddHttpContextAccessor();
        services.AddSingleton<ICultureProvider, HttpContextCultureProvider>();


        // Lokalizasyon kaynakları
        services.AddTransient<ILocalizationSourceAsync, JsonFileLocalizationSourceAsync>();

        // Ana servisler
        services.AddSingleton<ILocalizationServiceAsync, LocalizationServiceAsync>();
        services.AddSingleton<ICurrencyServiceAsync, CurrencyServiceAsync>();
        services.AddSingleton<IDateTimeServiceAsync, DateTimeServiceAsync>();
        services.AddSingleton<LocalizationSourceManagerAsync>();



        return services;
    }

    /// <summary>
    /// Lokalizasyon servislerini ekler
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <param name="configureOptions">Yapılandırma seçenekleri</param>
    /// <returns>Servis koleksiyonu</returns>
    public static IServiceCollection AddCoreLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions> configureOptions)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LocalizationPipelineBehavior<,>));


        // Yapılandırma seçeneklerini bağla
        services.Configure(configureOptions);

        // Önbellek servisi
        // Yerel bellek üzerinde cache için (test/dev ortamı):
        services.AddDistributedMemoryCache();

        services.AddHttpContextAccessor();
        services.AddSingleton<ICultureProvider, HttpContextCultureProvider>();

        // Lokalizasyon kaynakları
        services.AddTransient<ILocalizationSourceAsync, JsonFileLocalizationSourceAsync>();

        // Ana servisler
        services.AddSingleton<ILocalizationServiceAsync, LocalizationServiceAsync>();
        services.AddSingleton<ICurrencyServiceAsync, CurrencyServiceAsync>();
        services.AddSingleton<IDateTimeServiceAsync, DateTimeServiceAsync>();
        services.AddSingleton<LocalizationSourceManagerAsync>();



        return services;
    }
}
