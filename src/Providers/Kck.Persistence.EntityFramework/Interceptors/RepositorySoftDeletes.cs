using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// Remembers the entities a repository soft-deleted, per <see cref="DbContext"/>. They reach
/// <see cref="AuditInterceptor"/> as Modified, and the interceptor must not stamp them as modified, just as it does not
/// stamp the soft deletes it makes from <c>DbContext.Remove</c>.
/// </summary>
internal static class RepositorySoftDeletes
{
    private static readonly ConditionalWeakTable<DbContext, HashSet<object>> Marked = new();

    public static void Mark(DbContext context, IEnumerable<object> entities) =>
        Marked.GetValue(context, _ => new HashSet<object>(ReferenceEqualityComparer.Instance)).UnionWith(entities);

    public static void Unmark(DbContext context, object entity)
    {
        if (Marked.TryGetValue(context, out var entities))
            entities.Remove(entity);
    }

    public static bool IsMarked(DbContext context, object entity) =>
        Marked.TryGetValue(context, out var entities) && entities.Contains(entity);

    // Called only after a successful save, so a retried SaveChanges still treats them as soft deletes.
    public static void Clear(DbContext context) => Marked.Remove(context);
}
