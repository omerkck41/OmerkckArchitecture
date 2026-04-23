using System.Linq.Expressions;
using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Repositories;

/// <summary>
/// Encapsulates bulk update operations that bypass the change tracker.
/// </summary>
internal static class BulkOperationHelper
{
    /// <summary>
    /// Performs a bulk update using EF Core's ExecuteUpdateAsync (single SQL statement).
    /// <para>
    /// WARNING: Bypasses change tracker and interceptors (e.g., audit).
    /// </para>
    /// </summary>
    public static async Task BulkUpdateAsync<T, TId>(
        DbSet<T> dbSet,
        Expression<Func<T, bool>> predicate,
        (Expression<Func<T, object>> Property, object? Value)[] updates,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
        await dbSet.Where(predicate)
            .ExecuteUpdateAsync(setters =>
            {
                foreach (var (property, value) in updates)
                    setters.SetProperty(property, _ => value!);
            }, cancellationToken).ConfigureAwait(false);
    }
}
