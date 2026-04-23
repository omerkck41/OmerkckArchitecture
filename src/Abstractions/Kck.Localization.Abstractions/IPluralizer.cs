namespace Kck.Localization.Abstractions;

/// <summary>
/// Resolves CLDR plural category (zero, one, two, few, many, other) for a given count and culture.
/// </summary>
public interface IPluralizer
{
    /// <summary>
    /// Returns the plural category for the given count in the specified culture.
    /// </summary>
    /// <param name="count">The numeric count to evaluate.</param>
    /// <param name="culture">The culture code (e.g., "en", "tr", "ar").</param>
    /// <returns>One of: "zero", "one", "two", "few", "many", "other".</returns>
    string GetPluralCategory(int count, string culture);
}
