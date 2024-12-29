namespace Core.Persistence.Dynamic;

public class Filter
{
    public required string Field { get; set; }
    public required string Operator { get; set; }
    public object? Value { get; set; }
    public string? Logic { get; set; }
    public IEnumerable<Filter>? Filters { get; set; }

    public Filter() { }

    public Filter(string field, string @operator, object? value, string? logic, IEnumerable<Filter>? filters) : this()
    {
        Field = field;
        Operator = @operator;
        Value = value;
        Logic = logic;
        Filters = filters;
    }

    // Tür dönüşümü için yardımcı metot
    public T GetValue<T>()
    {
        return Value != null ? (T)Convert.ChangeType(Value, typeof(T)) : default!;
    }
}