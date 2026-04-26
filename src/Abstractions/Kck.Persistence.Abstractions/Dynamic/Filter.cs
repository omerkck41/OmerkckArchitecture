using System.Globalization;
using System.Text.Json;

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

    /// <summary>
    /// LS-FAZ-5 (4.4): Type-safe constructor — enum'u canonical lower-case
    /// wire string'e cevirir. Mevcut <c>string</c> ctor wire format ile geri uyumludur.
    /// </summary>
    /// <remarks>
    /// RS0026 supresyonu: bu ctor mevcut string-ctor'un birebir aynasi (semantik
    /// olarak ayni); ileride parametre eklemek/cikarmak gerekirse iki overload
    /// uyumlu olarak gunceenmelidir. PublicApiAnalyzers backward-compat uyarisi
    /// kasitli kabul edildi.
    /// </remarks>
#pragma warning disable RS0026
    public Filter(string field, FilterOperator @operator, object? value, string? logic = null, IEnumerable<Filter>? filters = null)
        : this(field, @operator.ToString().ToLowerInvariant(), value, logic, filters) { }
#pragma warning restore RS0026

    /// <summary>
    /// LS-FAZ-5 (4.4): String <see cref="Operator"/>'u <see cref="FilterOperator"/>
    /// enum'una parse eder; tanimsiz operator icin <c>null</c> doner.
    /// Wire format ihtiyaci icin string field korundu, bu property convenience'tir.
    /// </summary>
    public FilterOperator? OperatorEnum => TryParseOperator(Operator);

    /// <summary>
    /// LS-FAZ-5 (4.4): String operator'u case-insensitive parse eder.
    /// Tanimsiz veya null icin <c>null</c> doner — exception atmaz.
    /// </summary>
    public static FilterOperator? TryParseOperator(string? @operator) =>
        !string.IsNullOrWhiteSpace(@operator) &&
        Enum.TryParse<FilterOperator>(@operator, ignoreCase: true, out var op)
            ? op
            : null;

    /// <summary>
    /// LS-FAZ-5 (5.5): AOT-uyumlu, type-spesifik parser. JSON deserialize sonrasi
    /// gelen <see cref="JsonElement"/> ve <see cref="string"/> degerleri tipe gore
    /// donusturur; bilinmeyen tipler icin <c>Convert.ChangeType</c> fallback'i ile
    /// geri-uyumlulugu korur.
    /// </summary>
    public T GetValue<T>() => Value switch
    {
        null => default!,
        T direct => direct,
        JsonElement el => el.Deserialize<T>()!,
        string s => ParseString<T>(s),
        _ => (T)Convert.ChangeType(Value, typeof(T), CultureInfo.InvariantCulture),
    };

    private static T ParseString<T>(string s)
    {
        var type = typeof(T);
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(string)) return (T)(object)s;
        if (underlying == typeof(Guid)) return (T)(object)Guid.Parse(s);
        if (underlying == typeof(int)) return (T)(object)int.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(long)) return (T)(object)long.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(short)) return (T)(object)short.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(double)) return (T)(object)double.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(decimal)) return (T)(object)decimal.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(float)) return (T)(object)float.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(bool)) return (T)(object)bool.Parse(s);
        if (underlying == typeof(DateTime)) return (T)(object)DateTime.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(DateTimeOffset)) return (T)(object)DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(DateOnly)) return (T)(object)DateOnly.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(TimeOnly)) return (T)(object)TimeOnly.Parse(s, CultureInfo.InvariantCulture);
        if (underlying.IsEnum) return (T)Enum.Parse(underlying, s, ignoreCase: true);
        return (T)Convert.ChangeType(s, underlying, CultureInfo.InvariantCulture);
    }
}
