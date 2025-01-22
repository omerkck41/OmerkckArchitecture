namespace Core.Persistence.Dynamic;

public class FilterValidator
{
    public static bool Validate(Filter filter)
    {
        // Örnek doğrulama
        return !string.IsNullOrWhiteSpace(filter.Field) &&
               !string.IsNullOrWhiteSpace(filter.Operator);
    }
}