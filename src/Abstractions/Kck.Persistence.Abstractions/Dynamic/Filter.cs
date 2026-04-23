namespace Kck.Persistence.Abstractions.Dynamic;

public sealed class Filter
{
    public required string Field { get; set; }
    public required string Operator { get; set; }
    public object? Value { get; set; }
    public string? Logic { get; set; }
    public IEnumerable<Filter>? Filters { get; set; }

    public Filter() { }

    public Filter(string field, string @operator, object? value, string? logic = null, IEnumerable<Filter>? filters = null)
    {
        Field = field;
        Operator = @operator;
        Value = value;
        Logic = logic;
        Filters = filters;
    }

    public T GetValue<T>() =>
        Value is not null ? (T)Convert.ChangeType(Value, typeof(T), System.Globalization.CultureInfo.InvariantCulture) : default!;
}
