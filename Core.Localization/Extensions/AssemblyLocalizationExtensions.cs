using Core.Localization.Abstractions;
using Core.Localization.Configuration;
using Core.Localization.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Core.Localization.Extensions;

/// <summary>
/// Assembly tabanlı lokalizasyon desteği için extension metotları
/// </summary>
public static class AssemblyLocalizationExtensions
{
    /// <summary>
    /// Belirtilen assemblylerin 'Features' dizinlerinde lokalizasyon kaynaklarını tarar
    /// </summary>
    public static IServiceCollection AddAssemblyBasedLocalization(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Temel lokalizasyon ekle
        services.AddCoreLocalization(options =>
        {
            options.ResourcePaths = new List<string> { "Features" };
            options.EnableAutoDiscovery = true;
            options.UseFileSystemWatcher = false; // Assembly bazlı yaklaşımda buna gerek yok
        });

        // Assembly Resource Provider'ı ekle
        services.AddSingleton<IResourceProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LocalizationOptions>>();
            var logger = sp.GetRequiredService<ILogger<AssemblyResourceProvider>>();

            var assembliesToScan = assemblies.Length > 0
                ? assemblies
                : new[] { Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly() };

            return new AssemblyResourceProvider(options, logger, assembliesToScan);
        });

        return services;
    }

    /// <summary>
    /// Çalışan uygulamanın 'Features' dizininde lokalizasyon kaynaklarını tarar
    /// </summary>
    public static IServiceCollection AddApplicationLocalization(
        this IServiceCollection services)
    {
        // Çalışan uygulamanın assembly'sini kullan
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            throw new InvalidOperationException("Entry assembly bulunamadı");
        }

        return services.AddAssemblyBasedLocalization(entryAssembly);
    }

    /// <summary>
    /// Türü verilen bir sınıfın bulunduğu assembly'nin 'Features' dizininde lokalizasyon kaynaklarını tarar
    /// </summary>
    public static IServiceCollection AddLocalizationFromAssemblyOf<T>(
        this IServiceCollection services)
    {
        return services.AddAssemblyBasedLocalization(typeof(T).Assembly);
    }

    /// <summary>
    /// Birden fazla assembly için özel yol desenleriyle lokalizasyon kaynaklarını tarar
    /// </summary>
    public static IServiceCollection AddAdvancedAssemblyLocalization(
        this IServiceCollection services,
        Action<AssemblyLocalizationOptions> configureOptions)
    {
        var options = new AssemblyLocalizationOptions();
        configureOptions(options);

        services.AddCoreLocalization(o =>
        {
            o.ResourcePaths = options.ResourcePaths;
            o.FeatureDirectoryPattern = options.FeatureDirectoryPattern;
            o.EnableAutoDiscovery = true;
            o.UseFileSystemWatcher = false;
            o.EnableDebugLogging = options.EnableDebugLogging;
        });

        services.AddSingleton<IResourceProvider>(sp =>
        {
            var locOptions = sp.GetRequiredService<IOptions<LocalizationOptions>>();
            var logger = sp.GetRequiredService<ILogger<AssemblyResourceProvider>>();
            return new AssemblyResourceProvider(locOptions, logger, options.Assemblies);
        });

        return services;
    }

    /// <summary>
    /// Gösterdiğiniz örneğe benzer geriye dönük uyumlu yaklaşım
    /// </summary>
    public static IServiceCollection AddYamlResourceLocalization(this IServiceCollection services)
    {
        // Temel core lokalizasyon servislerini ekle
        services.AddCoreLocalization(options =>
        {
            options.ResourcePaths = new List<string> { "Features" };
            options.EnableAutoDiscovery = true;
            options.UseFileSystemWatcher = false;
            options.ResourceFileExtensions = new List<string> { "yaml", "yml", "json" };
        });

        // Çalışan uygulamanın assembly'si
        var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        // Assembly bazlı provider'ı ekle
        services.AddSingleton<IResourceProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LocalizationOptions>>();
            var logger = sp.GetRequiredService<ILogger<AssemblyResourceProvider>>();
            return new AssemblyResourceProvider(options, logger, new[] { entryAssembly }, 500);
        });

        return services;
    }
}

/// <summary>
/// Assembly bazlı lokalizasyon yapılandırma seçenekleri
/// </summary>
public class AssemblyLocalizationOptions
{
    /// <summary>
    /// Taranacak assembly'ler
    /// </summary>
    public ICollection<Assembly> Assemblies { get; } = new List<Assembly>();

    /// <summary>
    /// Taranacak dizin yolları
    /// </summary>
    public IReadOnlyList<string> ResourcePaths { get; set; } = new List<string> { "Features" };

    /// <summary>
    /// Feature dizin deseni
    /// </summary>
    public string FeatureDirectoryPattern { get; set; } = "**/Resources/Locales";

    /// <summary>
    /// Debug loglamayı etkinleştir
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Assembly ekle
    /// </summary>
    public AssemblyLocalizationOptions AddAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
        return this;
    }

    /// <summary>
    /// T türünün bulunduğu assembly'yi ekle
    /// </summary>
    public AssemblyLocalizationOptions AddAssemblyOf<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }

    /// <summary>
    /// Çalışan uygulamanın assembly'sini ekle
    /// </summary>
    public AssemblyLocalizationOptions AddEntryAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            Assemblies.Add(entryAssembly);
        }
        return this;
    }
}
