using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using System.Linq.Expressions;

namespace Core.Persistence.Dynamic;

public static class IQueryableDynamicFilterExtensions
{
    private static Expression<Func<T, object>> CreateOrderExpression<T>(string field)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, field);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<T, object>>(conversion, parameter);
    }
    private static Expression? BuildExpression<T>(ParameterExpression parameter, Filter filter)
    {
        var property = Expression.Property(parameter, filter.Field);
        var value = Expression.Constant(filter.GetValue<object>());

        var operatorEnum = Enum.Parse<FilterOperator>(filter.Operator, ignoreCase: true);

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
            FilterOperator.IsNull => Expression.Equal(property, Expression.Constant(null)),
            FilterOperator.IsNotNull => Expression.NotEqual(property, Expression.Constant(null)),
            _ => throw new CustomException($"Operator '{filter.Operator}' is not supported.")
        };
    }

    /// <summary>
    /// Dinamik sorguları filtre ve sıralamayı uygulayarak oluşturur.
    /// </summary>
    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, Dynamic dynamic)
    {
        if (dynamic.Filter is not null)
            query = ApplyFilter(query, dynamic.Filter);

        if (dynamic.Sort is not null && dynamic.Sort.Any())
            query = ApplySort(query, dynamic.Sort);

        return query;
    }

    /// <summary>
    /// Dinamik filtre uygular.
    /// </summary>
    private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, Filter filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var expression = BuildExpression<T>(parameter, filter);

        if (expression == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Dinamik sıralama uygular.
    /// </summary>
    private static IQueryable<T> ApplySort<T>(IQueryable<T> query, IEnumerable<Sort> sort)
    {
        if (!sort.Any())
            return query;

        var firstSort = sort.First();
        var orderedQuery = firstSort.Dir == "asc"
            ? query.OrderBy(CreateOrderExpression<T>(firstSort.Field))
            : query.OrderByDescending(CreateOrderExpression<T>(firstSort.Field));

        foreach (var nextSort in sort.Skip(1))
        {
            orderedQuery = nextSort.Dir == "asc"
                ? ((IOrderedQueryable<T>)orderedQuery).ThenBy(CreateOrderExpression<T>(nextSort.Field))
                : ((IOrderedQueryable<T>)orderedQuery).ThenByDescending(CreateOrderExpression<T>(nextSort.Field));
        }

        return orderedQuery;
    }
}
