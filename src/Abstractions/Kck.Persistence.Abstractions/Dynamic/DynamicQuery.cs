namespace Kck.Persistence.Abstractions.Dynamic;

/// <summary>
/// Represents a runtime-constructed query carrying sort instructions and an optional filter tree.
/// Used to translate client-supplied query parameters into repository operations without exposing raw predicates.
/// </summary>
public sealed class DynamicQuery
{
    /// <summary>
    /// The ordered collection of sort directives to apply to the query result.
    /// </summary>
    public IEnumerable<Sort> Sort { get; set; } = [];

    /// <summary>
    /// The root filter node; <c>null</c> means no filtering is applied.
    /// </summary>
    public Filter? Filter { get; set; }

    /// <summary>
    /// Initializes an empty instance with no sort directives and no filter.
    /// </summary>
    public DynamicQuery() { }

    /// <summary>
    /// Initializes a new instance with the specified sort directives and filter.
    /// </summary>
    /// <param name="sort">Sort directives; <c>null</c> is treated as an empty collection.</param>
    /// <param name="filter">The root filter node; pass <c>null</c> to skip filtering.</param>
    public DynamicQuery(IEnumerable<Sort>? sort, Filter? filter)
    {
        Sort = sort ?? [];
        Filter = filter;
    }
}
