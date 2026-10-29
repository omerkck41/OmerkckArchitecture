using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kck.Persistence.EntityFramework.Repositories;

/// <summary>
/// Turns deletes into soft deletes, for repository deletes and for <see cref="AuditInterceptor"/>. A soft delete
/// covers the rows a hard delete would remove (EF's cascade on tracked dependents): soft-deletable rows are
/// soft-deleted, the others are kept together with everything below them.
/// </summary>
internal static class SoftDeleteHelper
{
    private enum Outcome
    {
        Delete,
        SoftDelete,
        Keep,
    }

    /// <summary>Soft-deletes the given entities and the dependents EF cascades to.</summary>
    public static void SoftDelete(DbContext context, IEnumerable<object> entities, DateTime now)
    {
        var owners = new HashSet<object>(entities, ReferenceEqualityComparer.Instance);
        foreach (var entry in owners.Select(context.Entry))
        {
            // As DbContext.Remove: a row that was never inserted is just not inserted.
            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;
        }

        Apply(context, owners, now);
    }

    /// <summary>Turns every tracked delete of a soft-deletable entity into a soft delete, except permanent ones.</summary>
    /// <returns>The entities that were soft-deleted.</returns>
    public static IReadOnlySet<object> SoftDeleteTrackedDeletes(DbContext context, DateTime now) =>
        Apply(context, owners: null, now);

    // owners null: every deleted entry that no deleted principal cascades to is a root of its own. Otherwise only the
    // owners are soft-deleted; other deletes stay hard deletes, and so do the rows they cascade to.
    private static HashSet<object> Apply(DbContext context, HashSet<object>? owners, DateTime now)
    {
        var softDeleted = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var entries = context.ChangeTracker.Entries().ToList();
        var deleted = entries.Where(e => e.State == EntityState.Deleted).ToList();
        if (deleted.Count == 0)
            return softDeleted;

        var outcomes = new Dictionary<object, Outcome>(ReferenceEqualityComparer.Instance);
        foreach (var entry in deleted)
            Resolve(context, entry, deleted, owners, outcomes);

        var kept = new List<EntityEntry>();
        foreach (var entry in deleted)
        {
            switch (outcomes[entry.Entity])
            {
                case Outcome.SoftDelete:
                    var softDeletable = (ISoftDeletable)entry.Entity;
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedDate = now;
                    entry.State = EntityState.Modified;
                    softDeleted.Add(entry.Entity);
                    kept.Add(entry);
                    break;
                case Outcome.Keep:
                    entry.State = EntityState.Unchanged;
                    kept.Add(entry);
                    break;
            }
        }

        RestoreNulledForeignKeys(entries, kept);
        return softDeleted;
    }

    private static Outcome Resolve(
        DbContext context,
        EntityEntry entry,
        List<EntityEntry> deleted,
        HashSet<object>? owners,
        Dictionary<object, Outcome> outcomes)
    {
        if (outcomes.TryGetValue(entry.Entity, out var known))
            return known;

        outcomes[entry.Entity] = Outcome.Delete; // a cycle of deletes stays deleted
        var outcome = Decide(context, entry, deleted, owners, outcomes);
        outcomes[entry.Entity] = outcome;
        return outcome;
    }

    private static Outcome Decide(
        DbContext context,
        EntityEntry entry,
        List<EntityEntry> deleted,
        HashSet<object>? owners,
        Dictionary<object, Outcome> outcomes)
    {
        if (owners?.Contains(entry.Entity) == true)
            return Outcome.SoftDelete;

        if (PermanentDeletes.IsMarked(context, entry.Entity))
            return Outcome.Delete;

        var principals = DeletedPrincipals(entry, deleted)
            .Select(p => Resolve(context, p, deleted, owners, outcomes))
            .ToList();

        if (principals.Count == 0)
            return owners is null && entry.Entity is ISoftDeletable ? Outcome.SoftDelete : Outcome.Delete;

        // A row that stays deleted takes its dependents with it; a kept row keeps them.
        if (principals.Contains(Outcome.Delete))
            return Outcome.Delete;

        return entry.Entity is ISoftDeletable && principals.Contains(Outcome.SoftDelete)
            ? Outcome.SoftDelete
            : Outcome.Keep;
    }

    private static IEnumerable<EntityEntry> DeletedPrincipals(EntityEntry dependent, List<EntityEntry> deleted) =>
        dependent.Metadata.GetForeignKeys().SelectMany(foreignKey => deleted.Where(principal =>
            principal != dependent
            && foreignKey.PrincipalEntityType.IsAssignableFrom(principal.Metadata)
            && ReferencesPrincipal(dependent, principal, foreignKey)));

    // The cascade set an optional foreign key to null on dependents of a row that stays: link them to it again.
    private static void RestoreNulledForeignKeys(List<EntityEntry> entries, List<EntityEntry> kept)
    {
        foreach (var dependent in entries.Where(e => e.State == EntityState.Modified))
        {
            foreach (var foreignKey in dependent.Metadata.GetForeignKeys())
            {
                if (kept.Any(principal => foreignKey.PrincipalEntityType.IsAssignableFrom(principal.Metadata)
                                          && ReferencesPrincipal(dependent, principal, foreignKey)))
                    RestoreNulledForeignKey(dependent, foreignKey);
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

    // EF nulls only the nullable parts of a composite key; the others keep their original value. Any other current
    // value means the key was changed on purpose (moved to another principal), so it stays.
    private static void RestoreNulledForeignKey(EntityEntry dependent, IForeignKey foreignKey)
    {
        var properties = foreignKey.Properties.Select(p => dependent.Property(p.Name)).ToList();
        var nulled = properties.Where(p => p.CurrentValue is null).ToList();
        if (nulled.Count == 0 || properties.Except(nulled).Any(p =>
                !p.Metadata.GetKeyValueComparer().Equals(p.CurrentValue, p.OriginalValue)))
            return;

        foreach (var property in nulled)
        {
            property.CurrentValue = property.OriginalValue;
            property.IsModified = false;
        }
    }
}
