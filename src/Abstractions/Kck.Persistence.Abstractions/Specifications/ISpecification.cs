using System.Linq.Expressions;

namespace Kck.Persistence.Abstractions.Specifications;

/// <summary>
/// Defines the Specification pattern contract for encapsulating query logic for entity type <typeparamref name="T"/>.
/// Repositories consume implementations to build strongly-typed, composable queries without leaking predicate logic into callers.
/// </summary>
/// <typeparam name="T">The entity type the specification targets.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// The filter predicate applied in the WHERE clause; <c>null</c> means all records are returned.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Strongly-typed navigation properties to eagerly load (INCLUDE).
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// String-based navigation paths to eagerly load, used when the expression form is not practical.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// Ascending sort expression; <c>null</c> means no explicit ordering is applied.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Descending sort expression; <c>null</c> means no explicit ordering is applied.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Maximum number of records to return; <c>null</c> means no limit.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Number of records to skip before returning results; <c>null</c> means start from the first record.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// When <c>true</c>, soft-deleted records are included in the result set.
    /// </summary>
    bool WithDeleted { get; }

    /// <summary>
    /// When <c>true</c>, the query is executed with change tracking enabled; use for read-write scenarios.
    /// </summary>
    bool EnableTracking { get; }
}
