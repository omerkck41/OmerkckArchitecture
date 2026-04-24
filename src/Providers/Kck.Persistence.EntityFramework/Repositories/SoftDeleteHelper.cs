using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Repositories;

/// <summary>
/// Encapsulates cascade soft-delete logic for navigation properties.
/// </summary>
internal static class SoftDeleteHelper
{
    /// <summary>
    /// Marks all <see cref="ISoftDeletable"/> navigation children of the given entity as deleted.
    /// </summary>
    public static void CascadeSoftDelete(DbContext context, object entity)
    {
        var navigations = context.Entry(entity).Navigations
            .Where(n => n.CurrentValue is not null);

        foreach (var navigation in navigations)
        {
            if (navigation.CurrentValue is IEnumerable<object> collection)
            {
                foreach (var related in collection.OfType<ISoftDeletable>())
                {
                    related.IsDeleted = true;
                    related.DeletedDate = DateTime.UtcNow;
                    context.Entry(related).State = EntityState.Modified;
                }
            }
            else if (navigation.CurrentValue is ISoftDeletable related)
            {
                related.IsDeleted = true;
                related.DeletedDate = DateTime.UtcNow;
                context.Entry(navigation.CurrentValue).State = EntityState.Modified;
            }
        }
    }
}
