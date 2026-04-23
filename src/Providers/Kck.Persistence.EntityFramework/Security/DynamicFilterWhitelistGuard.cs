using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.Abstractions.Security;

namespace Kck.Persistence.EntityFramework.Security;

/// <summary>
/// Validates dynamic filter fields against a whitelist to prevent unauthorized property access.
/// </summary>
public static class DynamicFilterWhitelistGuard
{
    /// <summary>
    /// Validates that all filter and sort fields in the <paramref name="query"/>
    /// are allowed by the <paramref name="whitelist"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a disallowed field is referenced.</exception>
    public static void Validate<T>(DynamicQuery query, IFilterPropertyWhitelist<T> whitelist)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(whitelist);

        if (query.Filter is not null)
            ValidateFilter(query.Filter, whitelist);

        foreach (var sort in query.Sort)
            ValidateField(sort.Field, whitelist);
    }

    private static void ValidateFilter<T>(Filter filter, IFilterPropertyWhitelist<T> whitelist)
    {
        ValidateField(filter.Field, whitelist);

        if (filter.Filters is null)
            return;

        foreach (var nested in filter.Filters)
            ValidateFilter(nested, whitelist);
    }

    private static void ValidateField<T>(string field, IFilterPropertyWhitelist<T> whitelist)
    {
        if (!whitelist.IsAllowed(field))
            throw new ArgumentException(
                $"Filtering or sorting on property '{field}' is not allowed for type '{typeof(T).Name}'.");
    }
}
