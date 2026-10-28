using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// Remembers the entities a repository removed with <c>permanent: true</c>, per <see cref="DbContext"/>,
/// so <see cref="AuditInterceptor"/> does not turn those deletes into soft deletes.
/// </summary>
internal static class PermanentDeletes
{
    private static readonly ConditionalWeakTable<DbContext, HashSet<object>> Marked = new();

    public static void Mark(DbContext context, object entity) =>
        Marked.GetValue(context, _ => new HashSet<object>(ReferenceEqualityComparer.Instance)).Add(entity);

    public static bool IsMarked(DbContext context, object entity) =>
        Marked.TryGetValue(context, out var entities) && entities.Contains(entity);

    // Called only after a successful save, so a retried SaveChanges still deletes permanently.
    public static void Clear(DbContext context) => Marked.Remove(context);
}
