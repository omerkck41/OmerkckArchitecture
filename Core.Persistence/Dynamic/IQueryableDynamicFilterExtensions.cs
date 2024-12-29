using System.Linq.Dynamic.Core;

namespace Core.Persistence.Dynamic;

public static class IQueryableDynamicFilterExtensions
{
    // Operatörler için sabit bir dictionary
    private static readonly Dictionary<string, Func<string, string>> Operators = new()
    {
        { "eq", field => $"{field} == @" },
        { "neq", field => $"{field} != @" },
        { "lt", field => $"{field} < @" },
        { "lte", field => $"{field} <= @" },
        { "gt", field => $"{field} > @" },
        { "gte", field => $"{field} >= @" },
        { "isnull", field => $"{field} == null" },
        { "isnotnull", field => $"{field} != null" },
        { "startswith", field => $"{field}.StartsWith(@" },
        { "endswith", field => $"{field}.EndsWith(@" },
        { "contains", field => $"{field}.Contains(@" },
        { "doesnotcontain", field => $"!{field}.Contains(@" }
    };

    /// <summary>
    /// Dinamik sorguları filtre ve sıralamayı uygulayarak oluşturur.
    /// </summary>
    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, Dynamic dynamic)
    {
        var filterValidator = new FilterValidator();

        if (dynamic.Filter is not null && !filterValidator.Validate(dynamic.Filter))
            throw new ArgumentException("Invalid filter provided.");

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
        var filters = ExtractFilters(filter);
        string whereClause = CreateWhereClause(filter, filters);
        object?[] values = filters.Select(f => f.Value).ToArray();

        return query.Where(whereClause, values);
    }

    /// <summary>
    /// Dinamik sıralama uygular.
    /// </summary>
    private static IQueryable<T> ApplySort<T>(IQueryable<T> query, IEnumerable<Sort> sort)
    {
        string orderByClause = string.Join(", ", sort.OrderBy(s => s.Priority).Select(s => $"{s.Field} {s.Dir}"));
        return query.OrderBy(orderByClause);
    }

    private static List<Filter> ExtractFilters(Filter filter)
    {
        var filters = new List<Filter> { filter };

        if (filter.Filters != null && filter.Filters.Any())
        {
            foreach (var nestedFilter in filter.Filters)
                filters.AddRange(ExtractFilters(nestedFilter));
        }

        return filters;
    }

    /// <summary>
    /// Tüm filtreleri iç içe döngüden çıkarır.
    /// </summary>
    public static IList<Filter> ExtractAllFilters(Filter filter)
    {
        List<Filter> filters = [];
        CollectFilters(filter, filters);
        return filters;
    }

    private static void CollectFilters(Filter filter, IList<Filter> filters)
    {
        filters.Add(filter);

        if (filter.Filters == null || !filter.Filters.Any()) return;

        foreach (var nestedFilter in filter.Filters)
            CollectFilters(nestedFilter, filters);
    }

    /// <summary>
    /// Tüm filtreleri kullanarak WHERE ifadesi oluşturur.
    /// </summary>
    private static string CreateWhereClause(Filter filter, List<Filter> filters)
    {
        int index = filters.IndexOf(filter);
        string comparison = Operators[filter.Operator](filter.Field);
        return $"{comparison}{index}";
    }
}
