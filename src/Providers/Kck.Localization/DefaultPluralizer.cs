using Kck.Localization.Abstractions;

namespace Kck.Localization;

/// <summary>
/// Default pluralizer implementing basic CLDR plural rules for common languages.
/// </summary>
public sealed class DefaultPluralizer : IPluralizer
{
    public string GetPluralCategory(int count, string culture)
    {
        var lang = ExtractBaseLanguage(culture);

        return lang switch
        {
            "ar" => GetArabicPlural(count),
            "pl" => GetPolishPlural(count),
            "fr" or "pt" => GetFrenchPlural(count),
            _ => count == 1 ? "one" : "other",
        };
    }

    private static string ExtractBaseLanguage(string culture)
    {
        var dashIndex = culture.IndexOf('-');
        return dashIndex > 0 ? culture[..dashIndex].ToLowerInvariant() : culture.ToLowerInvariant();
    }

    private static string GetArabicPlural(int count)
    {
        if (count == 0) return "zero";
        if (count == 1) return "one";
        if (count == 2) return "two";

        var mod100 = count % 100;
        if (mod100 is >= 3 and <= 10) return "few";
        if (mod100 is >= 11 and <= 99) return "many";

        return "other";
    }

    private static string GetPolishPlural(int count)
    {
        if (count == 1) return "one";

        var mod10 = count % 10;
        var mod100 = count % 100;

        if (mod10 is >= 2 and <= 4 && mod100 is not (>= 12 and <= 14))
            return "few";

        return "many";
    }

    private static string GetFrenchPlural(int count)
    {
        if (count is 0 or 1) return "one";
        if (count != 0 && count % 1000000 == 0) return "many";
        return "other";
    }
}
