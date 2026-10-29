using System.Linq.Expressions;

namespace Kck.Persistence.Abstractions.Repositories;

/// <summary>
/// Low-level <see cref="IQueryable{T}"/> access for custom LINQ queries. Prefer <c>IReadRepository</c> where possible.
/// </summary>
public interface IQuery<T>
{
    /// <summary>
    /// Returns an <see cref="IQueryable{T}"/> filtered and sorted per the provided parameters.
    /// </summary>
    [Obsolete(
        "Positional bool flags are replaced by QueryOptions: use Query(QueryOptions.None, filter, orderBy) " +
        "(or QueryOptions.Tracking / QueryOptions.WithDeleted). Removed in v5.0.",
        DiagnosticId = "KCK0101",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    IQueryable<T> Query(Expression<Func<T, bool>>? filter = null,
                        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                        bool withDeleted = false,
                        bool enableTracking = false);

    /// <summary>
    /// Returns an <see cref="IQueryable{T}"/> filtered and sorted per the provided parameters.
    /// </summary>
    /// <param name="options">Soft-deleted rows and change tracking; <see cref="QueryOptions.None"/> excludes both.</param>
    /// <param name="filter">Optional predicate.</param>
    /// <param name="orderBy">Optional ordering.</param>
    IQueryable<T> Query(QueryOptions options,
                        Expression<Func<T, bool>>? filter = null,
                        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
#pragma warning disable KCK0101 // implementations written before v4.3 only have the member this one replaces
        return ForwardToDeprecatedMember();

        // Marked like the member it calls: the forwarding itself is part of the deprecated surface, removed in v5.0.
        [Obsolete("Forwards to the KCK0101 member until v5.0.", DiagnosticId = "KCK0101")]
        IQueryable<T> ForwardToDeprecatedMember() =>
            Query(filter, orderBy, options.IncludeDeleted, options.AsTracking);
#pragma warning restore KCK0101
    }
}
