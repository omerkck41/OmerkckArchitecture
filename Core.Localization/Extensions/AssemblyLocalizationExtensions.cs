using Core.Localization.Abstractions;
using Core.Localization.Configuration;
using Core.Localization.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Core.Localization.Extensions;

/// <summary>
/// Assembly-based localization support extension methods
/// </summary>
public static class AssemblyLocalizationExtensions
{
    /// <summary>
    /// Adds assembly-based localization by scanning the 'Features' directories in the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for resources</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAssemblyBasedLocalization(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Add core localization
        services.AddLocalization(options =>
        {
            options.ResourcePaths = new List<string> { "Features" };
            options.EnableAutoDiscovery = true;
            options.UseFileSystemWatcher = false; // No need for file system watcher with assembly-based approach
        });

        // Add assembly resource provider
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
    /// Adds assembly-based localization using the entry assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationLocalization(
        this IServiceCollection services)
    {
        // Use the entry assembly
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            throw new InvalidOperationException("Entry assembly not found");
        }

        return services.AddAssemblyBasedLocalization(entryAssembly);
    }

    /// <summary>
    /// Adds assembly-based localization using the assembly containing the specified type
    /// </summary>
    /// <typeparam name="T">The type to get the assembly from</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLocalizationFromAssemblyOf<T>(
        this IServiceCollection services)
    {
        return services.AddAssemblyBasedLocalization(typeof(T).Assembly);
    }

    /// <summary>
    /// Adds advanced assembly-based localization with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAdvancedAssemblyLocalization(
        this IServiceCollection services,
        Action<AssemblyLocalizationOptions> configureOptions)
    {
        var options = new AssemblyLocalizationOptions();
        configureOptions(options);

        services.AddLocalization(o =>
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
}

/// <summary>
/// Options for assembly-based localization
/// </summary>
public class AssemblyLocalizationOptions
{
    /// <summary>
    /// Assemblies to scan for resources
    /// </summary>
    public ICollection<Assembly> Assemblies { get; } = new List<Assembly>();

    /// <summary>
    /// Resource paths to scan
    /// </summary>
    public IReadOnlyList<string> ResourcePaths { get; set; } = new List<string> { "Features" };

    /// <summary>
    /// Feature directory pattern
    /// </summary>
    public string FeatureDirectoryPattern { get; set; } = "**/Resources/Locales";

    /// <summary>
    /// Whether to enable debug logging
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Adds an assembly to scan
    /// </summary>
    /// <param name="assembly">The assembly to add</param>
    /// <returns>The options for chaining</returns>
    public AssemblyLocalizationOptions AddAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
        return this;
    }

    /// <summary>
    /// Adds the assembly containing the specified type
    /// </summary>
    /// <typeparam name="T">The type to get the assembly from</typeparam>
    /// <returns>The options for chaining</returns>
    public AssemblyLocalizationOptions AddAssemblyOf<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }

    /// <summary>
    /// Adds the entry assembly
    /// </summary>
    /// <returns>The options for chaining</returns>
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
