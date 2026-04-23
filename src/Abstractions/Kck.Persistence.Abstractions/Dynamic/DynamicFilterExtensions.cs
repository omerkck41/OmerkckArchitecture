using System.Linq.Expressions;

namespace Kck.Persistence.Abstractions.Dynamic;

public static class DynamicFilterExtensions
{
    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamic)
    {
        if (dynamic.Filter is not null)
            query = ApplyFilter(query, dynamic.Filter);

        if (dynamic.Sort.Any())
            query = ApplySort(query, dynamic.Sort);

        return query;
    }

    private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, Filter filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var expression = BuildExpression<T>(parameter, filter);

        if (expression is null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
        return query.Where(lambda);
    }

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
        var operatorEnum = Enum.Parse<FilterOperator>(filter.Operator, ignoreCase: true);

        if (operatorEnum is FilterOperator.IsNull)
            return Expression.Equal(property, Expression.Constant(null));

        if (operatorEnum is FilterOperator.IsNotNull)
            return Expression.NotEqual(property, Expression.Constant(null));

        var value = Expression.Constant(filter.GetValue<object>());

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

    private static Expression<Func<T, object>> CreateOrderExpression<T>(string field)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, field);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<T, object>>(conversion, parameter);
    }
}
