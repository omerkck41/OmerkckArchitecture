using System.Globalization;
using System.Text.Json;

namespace Core.ToolKit.Localization;

public static class LocalizationHelper
{
    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private static string _defaultCulture = CultureInfo.CurrentCulture.Name;

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

        foreach (var (culture, filePath) in files)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Translation file not found at path: {filePath}");

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
                ?? throw new InvalidOperationException("Failed to deserialize translation file.");

            _translations[culture] = translations;
        }
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

        if (_translations.TryGetValue(culture, out var translations) && translations.TryGetValue(key, out var value))
        {
            return string.Format(value, parameters);
        }

        // Fallback to default culture
        return key;
    }

    /// <summary>
    /// Clears all loaded translations.
    /// </summary>
    public static void ClearTranslations()
    {
        _translations.Clear();
    }
}