using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace Core.ToolKit.Localization;

public static class LocalizationHelper
{
    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private static string _defaultCulture = CultureInfo.CurrentCulture.Name;
    private static readonly ConcurrentDictionary<string, string> _translationCache = new();

    public static string DefaultCulture
    {
        get => _defaultCulture;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Culture cannot be null or empty.", nameof(value));

            _defaultCulture = value;
        }
    }

    /// <summary>
    /// Loads translations from multiple JSON files for specified cultures.
    /// </summary>
    /// <param name="files">Dictionary of culture and file paths.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task LoadTranslationsAsync(Dictionary<string, string> files)
    {
        if (files == null || files.Count == 0)
            throw new ArgumentException("Files dictionary cannot be null or empty.", nameof(files));

        var tasks = files.Select(async file =>
        {
            if (string.IsNullOrWhiteSpace(file.Value))
                throw new ArgumentException("File path cannot be null or empty.", nameof(file.Value));

            if (!File.Exists(file.Value))
                throw new FileNotFoundException($"Translation file not found at path: {file.Value}");

            var jsonContent = await File.ReadAllTextAsync(file.Value);
            var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
                ?? throw new InvalidOperationException("Failed to deserialize translation file.");

            _translations[file.Key] = translations;
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Translates a key with support for fallbacks and interpolation.
    /// </summary>
    /// <param name="key">Translation key.</param>
    /// <param name="parameters">Optional parameters for string interpolation.</param>
    /// <returns>Translated value, or the key if no translation is found.</returns>
    public static string Translate(string key, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var culture = _defaultCulture;

        if (_translationCache.TryGetValue(key, out var cachedValue))
        {
            return string.Format(cachedValue, parameters);
        }

        if (_translations.TryGetValue(culture, out var translations) && translations.TryGetValue(key, out var value))
        {
            _translationCache[key] = value;
            return string.Format(value, parameters);
        }

        // Fallback to default culture (e.g., "en-US")
        if (culture != "en-US" && _translations.TryGetValue("en-US", out var defaultTranslations) && defaultTranslations.TryGetValue(key, out var defaultValue))
        {
            _translationCache[key] = defaultValue;
            return string.Format(defaultValue, parameters);
        }

        return key;
    }

    /// <summary>
    /// Clears all loaded translations.
    /// </summary>
    public static void ClearTranslations()
    {
        _translations.Clear();
        _translationCache.Clear();
    }

    public static IEnumerable<string> GetSupportedCultures()
    {
        return _translations.Keys;
    }

    public static void SetCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            throw new ArgumentException("Culture cannot be null or empty.", nameof(culture));

        if (!_translations.ContainsKey(culture))
            throw new ArgumentException($"Culture '{culture}' is not supported.", nameof(culture));

        _defaultCulture = culture;
    }
}