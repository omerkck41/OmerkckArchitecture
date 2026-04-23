using System.Collections.Frozen;

namespace Kck.Persistence.Abstractions.Dynamic;

public static class FilterValidator
{
    private static readonly FrozenSet<string> SupportedOperators =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "eq", "neq", "lt", "lte", "gt", "gte",
            "contains", "startswith", "endswith",
            "isnull", "isnotnull"
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool Validate(Filter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.Field))
            return false;

        if (string.IsNullOrWhiteSpace(filter.Operator) || !SupportedOperators.Contains(filter.Operator))
            return false;

        if (filter.Operator is "isnull" or "isnotnull")
        {
            if (filter.Value is not null)
                return false;
        }
        else
        {
            if (filter.Value is null)
                return false;
        }

        return true;
    }
}
