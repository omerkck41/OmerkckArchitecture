using System.Globalization;

namespace Core.Localization.Extensions;

/// <summary>
/// Extension methods for CultureInfo
/// </summary>
public static class CultureInfoExtensions
{
    /// <summary>
    /// Gets the two-letter ISO language code
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>Two-letter ISO language code</returns>
    public static string GetLanguageCode(this CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName;
    }

    /// <summary>
    /// Gets the full culture name (e.g., en-US)
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>Full culture name</returns>
    public static string GetFullName(this CultureInfo culture)
    {
        return culture.Name;
    }

    /// <summary>
    /// Checks if the culture is a neutral culture (e.g., en)
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>True if the culture is neutral, false otherwise</returns>
    public static bool IsNeutralCulture(this CultureInfo culture)
    {
        return culture.IsNeutralCulture;
    }

    /// <summary>
    /// Gets the parent culture or null if it's the invariant culture
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>Parent culture or null</returns>
    public static CultureInfo? GetParentCulture(this CultureInfo culture)
    {
        if (culture.Parent == CultureInfo.InvariantCulture)
        {
            return null;
        }
        return culture.Parent;
    }

    /// <summary>
    /// Gets all parent cultures up to the invariant culture
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>Enumerable of parent cultures</returns>
    public static IEnumerable<CultureInfo> GetParentCultures(this CultureInfo culture)
    {
        var current = culture.Parent;
        while (current != CultureInfo.InvariantCulture && !string.IsNullOrEmpty(current.Name))
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Checks if two cultures are the same or if one is a parent of the other
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <param name="otherCulture">The other culture to compare with</param>
    /// <returns>True if the cultures are related, false otherwise</returns>
    public static bool IsRelatedTo(this CultureInfo culture, CultureInfo otherCulture)
    {
        if (culture.Equals(otherCulture))
        {
            return true;
        }

        var current = culture.Parent;
        while (current != CultureInfo.InvariantCulture && !string.IsNullOrEmpty(current.Name))
        {
            if (current.Equals(otherCulture))
            {
                return true;
            }
            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Gets the display name in the culture's own language
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>Native display name</returns>
    public static string GetNativeDisplayName(this CultureInfo culture)
    {
        return culture.NativeName;
    }

    /// <summary>
    /// Gets the display name in English
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>English display name</returns>
    public static string GetEnglishDisplayName(this CultureInfo culture)
    {
        return culture.EnglishName;
    }

    /// <summary>
    /// Checks if the culture is right-to-left
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>True if the culture is right-to-left, false otherwise</returns>
    public static bool IsRightToLeft(this CultureInfo culture)
    {
        return culture.TextInfo.IsRightToLeft;
    }

    /// <summary>
    /// Gets the normalized culture code (e.g., "en" from "en-US")
    /// </summary>
    /// <param name="culture">The culture</param>
    /// <returns>Normalized culture code</returns>
    public static string GetNormalizedCultureCode(this CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName.ToLowerInvariant();
    }

    /// <summary>
    /// Creates a dictionary mapping of common currency symbols to their culture infos
    /// </summary>
    /// <returns>Dictionary of currency symbols to their culture infos</returns>
    public static IDictionary<string, CultureInfo> GetCommonCurrencySymbolMap()
    {
        return new Dictionary<string, CultureInfo>
        {
            { "€", CultureInfo.GetCultureInfo("fr-FR") }, // Euro
            { "$", CultureInfo.GetCultureInfo("en-US") }, // US Dollar
            { "£", CultureInfo.GetCultureInfo("en-GB") }, // British Pound
            { "¥", CultureInfo.GetCultureInfo("ja-JP") }, // Japanese Yen
            { "₺", CultureInfo.GetCultureInfo("tr-TR") }, // Turkish Lira
            { "₽", CultureInfo.GetCultureInfo("ru-RU") }, // Russian Ruble
            { "₹", CultureInfo.GetCultureInfo("hi-IN") }, // Indian Rupee
            { "元", CultureInfo.GetCultureInfo("zh-CN") }, // Chinese Yuan
            { "₩", CultureInfo.GetCultureInfo("ko-KR") }, // Korean Won
            { "₪", CultureInfo.GetCultureInfo("he-IL") }  // Israeli Shekel
        };
    }
}
