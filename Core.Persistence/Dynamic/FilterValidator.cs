namespace Core.Persistence.Dynamic;

public class FilterValidator
{
    public bool Validate(Filter filter)
    {
        // Örnek doğrulama
        return !string.IsNullOrWhiteSpace(filter.Field) &&
               !string.IsNullOrWhiteSpace(filter.Operator);
    }
}