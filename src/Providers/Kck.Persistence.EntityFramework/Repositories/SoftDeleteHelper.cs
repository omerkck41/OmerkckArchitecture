using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
        // Dependents only: a reference to a principal (which other rows may share) and the other side of a
        // many-to-many are not part of the entity being deleted.
        var navigations = context.Entry(entity).Navigations
            .Where(n => n.Metadata is INavigation { IsOnDependent: false } && n.CurrentValue is not null);

        foreach (var navigation in navigations) // NOSONAR S3267 - side-effect body; LINQ Select() would obscure intent
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
