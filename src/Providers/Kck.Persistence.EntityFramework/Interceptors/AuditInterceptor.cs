using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit fields on <see cref="IAuditable"/>
/// and converts hard deletes to soft deletes for <see cref="ISoftDeletable"/> entities, except deletes made
/// through a repository with <c>permanent: true</c>.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ApplyAuditFields(eventData.Context);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            ApplyAuditFields(eventData.Context);

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            PermanentDeletes.Clear(eventData.Context);

        return new ValueTask<int>(result);
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not null)
            PermanentDeletes.Clear(eventData.Context);

        return result;
    }

    private static void ApplyAuditFields(DbContext context)
    {
        var entries = context.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added when entry.Entity is IAuditable addedAuditable:
                    addedAuditable.CreatedDate = DateTime.UtcNow;
                    break;

                case EntityState.Modified when entry.Entity is IAuditable modifiedAuditable:
                    modifiedAuditable.ModifiedDate = DateTime.UtcNow;
                    break;

                case EntityState.Deleted when entry.Entity is ISoftDeletable softDeletable
                                              && !PermanentDeletes.IsMarked(context, entry.Entity):
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedDate = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                    KeepDependents(entry, entries);
                    break;
            }
        }
    }

    // Removing the owner made EF cascade to its tracked dependents through their foreign keys: required ones (with or
    // without a navigation, owned values, many-to-many links) were marked deleted, optional ones had the key set to
    // null. The owner row stays, so they stay linked to it; soft-deletable dependents are soft-deleted by the loop above.
    private static void KeepDependents(EntityEntry owner, List<EntityEntry> entries)
    {
        var candidates = entries.Where(e => e.State is EntityState.Deleted or EntityState.Modified).ToList();
        var principals = new Stack<EntityEntry>();
        principals.Push(owner);

        while (principals.Count > 0)
        {
            var principal = principals.Pop();
            foreach (var foreignKey in principal.Metadata.GetReferencingForeignKeys())
            {
                var dependents = candidates.Where(d => foreignKey.DeclaringEntityType.IsAssignableFrom(d.Metadata)
                                                       && ReferencesPrincipal(d, principal, foreignKey));
                foreach (var dependent in dependents)
                {
                    if (dependent.State == EntityState.Deleted && dependent.Entity is not ISoftDeletable)
                    {
                        dependent.State = EntityState.Unchanged;
                        principals.Push(dependent);
                    }
                    else if (dependent.State == EntityState.Modified)
                    {
                        RestoreNulledForeignKey(dependent, foreignKey);
                    }
                }
            }
        }
    }

    // Original values: a cascade that nulled an optional key leaves the link only there. EF's key comparers handle
    // byte[] keys and converted value types.
    private static bool ReferencesPrincipal(EntityEntry dependent, EntityEntry principal, IForeignKey foreignKey)
    {
        for (var i = 0; i < foreignKey.Properties.Count; i++)
        {
            var principalProperty = foreignKey.PrincipalKey.Properties[i];
            if (!principalProperty.GetKeyValueComparer().Equals(
                    dependent.Property(foreignKey.Properties[i].Name).OriginalValue,
                    principal.Property(principalProperty.Name).CurrentValue))
                return false;
        }

        return true;
    }

    private static void RestoreNulledForeignKey(EntityEntry dependent, IForeignKey foreignKey)
    {
        var properties = foreignKey.Properties.Select(p => dependent.Property(p.Name)).ToList();
        if (properties.Any(p => p.CurrentValue is not null))
            return;

        foreach (var property in properties)
        {
            property.CurrentValue = property.OriginalValue;
            property.IsModified = false;
        }
    }
}
