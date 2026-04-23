namespace Kck.Localization.Abstractions;

public sealed class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "en";
    public string FallbackCulture { get; set; } = "en";
    public IReadOnlyList<string> SupportedCultures { get; set; } = ["en"];
    public string ResourcePath { get; set; } = "Resources";
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>When true, GetStringAsync throws KeyNotFoundException on missing keys instead of returning the key.</summary>
    public bool ThrowOnMissing { get; set; }

    /// <summary>Pattern for missing key return value. {0} = key. Default: "[{0}]".</summary>
    public string MissingKeyPattern { get; set; } = "[{0}]";
}
