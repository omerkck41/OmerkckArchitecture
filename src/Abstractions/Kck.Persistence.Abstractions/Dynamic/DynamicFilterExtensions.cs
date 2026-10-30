using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;

namespace Kck.Persistence.Abstractions.Dynamic;

/// <summary>
/// Extension methods for applying <see cref="DynamicQuery"/> filters to <see cref="IQueryable{T}"/>.
/// </summary>
public static class DynamicFilterExtensions
{
    /// <summary>
    /// Applies the filter, sort, and page spec from a <see cref="DynamicQuery"/> to the queryable.
    /// </summary>
    [RequiresUnreferencedCode("Builds LINQ expressions from string field names using Expression.Property — fields must be preserved for trimming.")]
    [RequiresDynamicCode("Filter value deserialization via JsonElement.Deserialize<T> may require runtime code generation.")]
    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamic)
    {
        if (dynamic.Filter is not null)
            query = ApplyFilter(query, dynamic.Filter);

        if (dynamic.Sort.Any())
            query = ApplySort(query, dynamic.Sort);

        return query;
    }

    [RequiresUnreferencedCode("Builds LINQ expressions from string field names using Expression.Property.")]
    [RequiresDynamicCode("Filter value deserialization via JsonElement.Deserialize<T> may require runtime code generation.")]
    private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, Filter filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var expression = BuildExpression<T>(parameter, filter);

        if (expression is null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
        return query.Where(lambda);
    }

    [RequiresUnreferencedCode("Builds LINQ expressions from string field names using Expression.Property.")]
    [RequiresDynamicCode("Filter value deserialization via JsonElement.Deserialize<T> may require runtime code generation.")]
    private static Expression? BuildExpression<T>(ParameterExpression parameter, Filter filter)
    {
        // Group node: has child filters — combine them with the specified logic (default: "and")
        if (filter.Filters is not null)
        {
            var children = filter.Filters
                .Select(f => BuildExpression<T>(parameter, f))
                .Where(e => e is not null)
                .Select(e => e!)
                .ToList();

            if (children.Count == 0)
                return null;

            var logic = filter.Logic ?? "and";
            return logic.Equals("or", StringComparison.OrdinalIgnoreCase)
                ? children.Aggregate((left, right) => Expression.OrElse(left, right))
                : children.Aggregate((left, right) => Expression.AndAlso(left, right));
        }

        // Leaf node: Field + Operator + optional Value
        var property = Expression.Property(parameter, filter.Field);
        // LS-FAZ-5: Filter.OperatorEnum convenience getter — gecersiz operator icin null doner.
        var operatorEnum = filter.OperatorEnum
            ?? throw new ArgumentException($"Operator '{filter.Operator}' is not supported.", nameof(filter));

        if (operatorEnum is FilterOperator.IsNull)
            return Expression.Equal(property, Expression.Constant(null));

        if (operatorEnum is FilterOperator.IsNotNull)
            return Expression.NotEqual(property, Expression.Constant(null));

        // The constant must have the property's type: a value read from JSON is a JsonElement and one from a query string
        // is a string, and the comparison operators are not defined between those and, say, an enum or a Guid.
        var value = Expression.Constant(ConvertValue(filter.Value, property.Type, filter.Field), property.Type);

        try
        {
            return operatorEnum switch
            {
                FilterOperator.Eq => Expression.Equal(property, value),
                FilterOperator.Neq => Expression.NotEqual(property, value),
                FilterOperator.Lt => Expression.LessThan(property, value),
                FilterOperator.Lte => Expression.LessThanOrEqual(property, value),
                FilterOperator.Gt => Expression.GreaterThan(property, value),
                FilterOperator.Gte => Expression.GreaterThanOrEqual(property, value),
                FilterOperator.Contains => Expression.Call(property, typeof(string).GetMethod("Contains", [typeof(string)])!, value),
                FilterOperator.StartsWith => Expression.Call(property, typeof(string).GetMethod("StartsWith", [typeof(string)])!, value),
                FilterOperator.EndsWith => Expression.Call(property, typeof(string).GetMethod("EndsWith", [typeof(string)])!, value),
                _ => throw new ArgumentException($"Operator '{filter.Operator}' is not supported.")
            };
        }
        catch (InvalidOperationException ex) // the property's type does not define the operator, e.g. 'gt' on an enum
        {
            throw new ArgumentException(
                $"Operator '{filter.Operator}' is not supported for '{filter.Field}' ({property.Type.Name}).", nameof(filter), ex);
        }
    }

    // Converts a filter value to the property's type. A value that does not fit (an unknown enum name, a malformed Guid,
    // null for a non-nullable property, a JSON object) is the caller's input error: ArgumentException, like an unknown
    // field or operator.
    [RequiresUnreferencedCode("JsonElement.Deserialize(Type) and Enum conversions need the property type's metadata.")]
    [RequiresDynamicCode("JsonElement.Deserialize(Type) may require runtime code generation.")]
    private static object? ConvertValue(object? value, Type propertyType, string field)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        try
        {
            var converted = value switch
            {
                null or JsonElement { ValueKind: JsonValueKind.Null } => null,
                JsonElement { ValueKind: JsonValueKind.String } element => FromString(element.GetString()!, type),
                JsonElement { ValueKind: JsonValueKind.Number } element when type.IsEnum => Enum.ToObject(type, element.GetInt64()),
                JsonElement { ValueKind: JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False } element =>
                    element.Deserialize(type),
                JsonElement => throw new FormatException("Objects and arrays are not filter values."),
                string text => FromString(text, type),
                _ when type.IsInstanceOfType(value) => value,
                _ when type.IsEnum => Enum.ToObject(type, value),
                _ => Convert.ChangeType(value, type, CultureInfo.InvariantCulture),
            };

            if (converted is null && propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) is null)
                throw new FormatException("A non-nullable property cannot be compared with null.");

            return converted;
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or JsonException
                                       or ArgumentException or InvalidOperationException)
        {
            throw new ArgumentException($"Value '{value}' is not valid for '{field}' ({propertyType.Name}).", nameof(value), ex);
        }
    }

    private static object FromString(string text, Type type) =>
        type == typeof(string) ? text
        : type.IsEnum ? Enum.Parse(type, text, ignoreCase: true)
        : type == typeof(Guid) ? Guid.Parse(text)
        // UTC like the audit dates: a date without an offset is taken as UTC, one with an offset is converted (Npgsql
        // accepts only DateTimeKind.Utc for timestamptz).
        : type == typeof(DateTime)
            ? DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
        : type == typeof(DateTimeOffset) ? DateTimeOffset.Parse(text, CultureInfo.InvariantCulture)
        : type == typeof(DateOnly) ? DateOnly.Parse(text, CultureInfo.InvariantCulture)
        : type == typeof(TimeOnly) ? TimeOnly.Parse(text, CultureInfo.InvariantCulture)
        : type == typeof(TimeSpan) ? TimeSpan.Parse(text, CultureInfo.InvariantCulture)
        : Convert.ChangeType(text, type, CultureInfo.InvariantCulture);

    [RequiresUnreferencedCode("Builds LINQ sort expressions from string field names using Expression.Property.")]
    private static IQueryable<T> ApplySort<T>(IQueryable<T> query, IEnumerable<Sort> sort)
    {
        var sortList = sort.OrderBy(s => s.Priority).ToList();
        if (sortList.Count == 0)
            return query;

        var first = sortList[0];
        var orderedQuery = first.Dir.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(CreateOrderExpression<T>(first.Field))
            : query.OrderByDescending(CreateOrderExpression<T>(first.Field));

        for (var i = 1; i < sortList.Count; i++)
        {
            var next = sortList[i];
            orderedQuery = next.Dir.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? orderedQuery.ThenBy(CreateOrderExpression<T>(next.Field))
                : orderedQuery.ThenByDescending(CreateOrderExpression<T>(next.Field));
        }

        return orderedQuery;
    }

    [RequiresUnreferencedCode("Builds sort expressions from string field names using Expression.Property.")]
    private static Expression<Func<T, object>> CreateOrderExpression<T>(string field)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, field);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<T, object>>(conversion, parameter);
    }
}
