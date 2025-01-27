namespace Core.Persistence.Dynamic;

public class FilterValidator
{
    private static readonly string[] SupportedOperators = { "eq", "neq", "lt", "lte", "gt", "gte", "contains", "startswith", "endswith", "isnull", "isnotnull" };

    public static bool Validate(Filter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.Field))
            return false;

        if (string.IsNullOrWhiteSpace(filter.Operator) || !SupportedOperators.Contains(filter.Operator))
            return false;

        // Value kontrolü
        if (filter.Operator is "isnull" or "isnotnull")
        {
            // Bu operatörler için Value null olmalıdır.
            if (filter.Value != null)
                return false;
        }
        else
        {
            // Diğer operatörler için Value null olmamalıdır.
            if (filter.Value == null)
                return false;
        }

        return true;
    }
}