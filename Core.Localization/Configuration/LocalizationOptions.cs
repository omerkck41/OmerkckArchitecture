using System.Globalization;

namespace Core.Localization.Configuration;

/// <summary>
/// Configuration options for the localization system
/// </summary>
public class LocalizationOptions
{
    /// <summary>
    /// Default culture to use when no culture is specified
    /// </summary>
    public CultureInfo DefaultCulture { get; set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// Fallback culture to use when a resource is not found in the requested culture
    /// </summary>
    public CultureInfo FallbackCulture { get; set; } = CultureInfo.GetCultureInfo("en-US");

    /// <summary>
    /// Supported cultures by the application
    /// </summary>
    public IReadOnlyList<CultureInfo> SupportedCultures { get; set; } = new List<CultureInfo>
    {
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("tr-TR")
    };

    /// <summary>
    /// Whether to use fallback culture when resource not found
    /// </summary>
    public bool UseFallbackCulture { get; set; } = true;

    /// <summary>
    /// Whether to throw exception when resource not found
    /// </summary>
    public bool ThrowOnMissingResource { get; set; } = false;

    /// <summary>
    /// Whether to cache resources for better performance
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache expiration time
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Resource file locations - this can be root directories where Feature folders exist
    /// </summary>
    public IReadOnlyList<string> ResourcePaths { get; set; } = new List<string>
    {
        "Resources",
        "Features"
    };

    /// <summary>
    /// Pattern for finding resource keys in strings (e.g., [key])
    /// </summary>
    public string ResourceKeyPattern { get; set; } = "{0}";

    /// <summary>
    /// Whether to monitor resource files for changes
    /// </summary>
    public bool EnableResourceFileWatching { get; set; } = true;

    /// <summary>
    /// Whether to enable debug logging
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;

    /// <summary>
    /// Custom resource name generator function
    /// </summary>
    public Func<string, CultureInfo, string>? ResourceNameGenerator { get; set; }

    /// <summary>
    /// Directory pattern for feature-based localization
    /// </summary>
    public string FeatureDirectoryPattern { get; set; } = "**/Resources/Locales";

    /// <summary>
    /// File pattern for feature-based localization
    /// E.g., "{section}.{culture}.{extension}" where:
    /// - {section} will be replaced with the section name (e.g., "users")
    /// - {culture} will be replaced with the culture code (e.g., "en", "tr")
    /// - {extension} will be replaced with the file extension (e.g., "yaml", "json")
    /// </summary>
    public string FeatureFilePattern { get; set; } = "{section}.{culture}.{extension}";

    /// <summary>
    /// Default section name when not specified
    /// </summary>
    public string DefaultSection { get; set; } = "Messages";

    /// <summary>
    /// Whether to use auto-discovery for feature resources
    /// When enabled, the system will scan all ResourcePaths to find resources
    /// </summary>
    public bool EnableAutoDiscovery { get; set; } = true;

    /// <summary>
    /// Auto-reload interval for feature resources
    /// </summary>
    public TimeSpan AutoReloadInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Resource file extensions to look for
    /// </summary>
    public IReadOnlyList<string> ResourceFileExtensions { get; set; } = new List<string>
    {
        "yaml",
        "yml",
        "json"
    };

    /// <summary>
    /// Section key in resource files
    /// </summary>
    public string SectionKey { get; set; } = "SectionName";

    /// <summary>
    /// Whether to use file system watcher for detecting changes in resources
    /// </summary>
    public bool UseFileSystemWatcher { get; set; } = true;
}
