using System.Linq.Expressions;

namespace Kck.Persistence.Abstractions.Specifications;

/// <summary>
/// Abstract base implementation of <see cref="ISpecification{T}"/> that provides protected builder methods
/// for constructing query specifications in derived classes.
/// </summary>
/// <typeparam name="T">The entity type the specification targets.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];
    private readonly List<string> _includeStrings = [];

    /// <inheritdoc/>
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<string> IncludeStrings => _includeStrings.AsReadOnly();

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc/>
    public int? Take { get; private set; }

    /// <inheritdoc/>
    public int? Skip { get; private set; }

    /// <inheritdoc/>
    public bool WithDeleted { get; private set; }

    /// <inheritdoc/>
    public bool EnableTracking { get; private set; }

    /// <summary>
    /// Sets the filter predicate applied in the WHERE clause.
    /// </summary>
    /// <param name="criteria">The filter expression to apply.</param>
    protected void Where(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    /// <summary>
    /// Adds a strongly-typed navigation property to eager-load.
    /// </summary>
    /// <param name="includeExpression">The navigation property expression to include.</param>
    protected void AddInclude(Expression<Func<T, object>> includeExpression) => _includes.Add(includeExpression);

    /// <summary>
    /// Adds a string-based navigation path to eager-load.
    /// </summary>
    /// <param name="includeString">The dotted navigation path (e.g., <c>"Order.Items"</c>).</param>
    protected void AddInclude(string includeString) => _includeStrings.Add(includeString);

    /// <summary>
    /// Sets the ascending sort expression for the query.
    /// </summary>
    /// <param name="orderBy">The property expression to sort by ascending.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;

    /// <summary>
    /// Sets the descending sort expression for the query.
    /// </summary>
    /// <param name="orderByDescending">The property expression to sort by descending.</param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescending) => OrderByDescending = orderByDescending;

    /// <summary>
    /// Applies skip/take pagination to the query.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Maximum number of records to return.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Instructs the repository to include soft-deleted records in the result set.
    /// </summary>
    protected void IncludeDeleted() => WithDeleted = true;

    /// <summary>
    /// Enables change tracking for the query; use when the returned entities will be modified and saved.
    /// </summary>
    protected void AsTracking() => EnableTracking = true;
}
