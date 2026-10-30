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
        var attached = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var owner in owners)
        {
            // As DbContext.Remove: a detached graph is tracked first, so the cascade reaches its loaded dependents; a
            // row that was never inserted is just not inserted.
            var entry = context.Entry(owner);
            if (entry.State == EntityState.Detached)
            {
                var tracked = context.ChangeTracker.Entries().Select(e => e.Entity).ToHashSet(ReferenceEqualityComparer.Instance);
                context.Attach(owner);
                attached.UnionWith(context.ChangeTracker.Entries().Select(e => e.Entity).Where(e => !tracked.Contains(e)));
            }

            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;
        }

        Apply(context, owners, now, attached);
    }

    /// <summary>Turns every tracked delete of a soft-deletable entity into a soft delete, except permanent ones.</summary>
    /// <returns>The entities that were soft-deleted.</returns>
    public static IReadOnlySet<object> SoftDeleteTrackedDeletes(DbContext context, DateTime now) =>
        Apply(context, owners: null, now, attached: null);

    // owners null: every deleted entry that no deleted principal cascades to is a root of its own. Otherwise only the
    // owners are soft-deleted; other deletes stay hard deletes, and so do the rows they cascade to. attached: entities
    // the caller passed in detached; their IsDeleted / DeletedDate values are not trusted.
    private static HashSet<object> Apply(DbContext context, HashSet<object>? owners, DateTime now, HashSet<object>? attached)
    {
        var softDeleted = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var entries = context.ChangeTracker.Entries().ToList();
        var deleted = entries.Where(e => e.State == EntityState.Deleted).ToList();

        // Only hard deletes: nothing to decide.
        if (owners is null && !deleted.Any(e => e.Entity is ISoftDeletable && !PermanentDeletes.IsMarked(context, e.Entity)))
            return softDeleted;

        var resolver = new Resolver(context, deleted, owners);
        foreach (var entry in deleted)
            resolver.Resolve(entry);
        var outcomes = resolver.Outcomes;

        var kept = new List<EntityEntry>();
        foreach (var entry in deleted)
        {
            switch (outcomes[entry.Entity])
            {
                case Outcome.SoftDelete:
                    // A row that is already soft-deleted keeps its DeletedDate: deleting it again is not a new deletion.
                    // (EF resets original values on the way to Deleted, so "already deleted" is IsDeleted with a date,
                    // and only for an entity that was loaded, not one passed in detached.)
                    var softDeletable = (ISoftDeletable)entry.Entity;
                    if (attached?.Contains(entry.Entity) == true || !softDeletable.IsDeleted || softDeletable.DeletedDate is null)
                    {
                        softDeletable.IsDeleted = true;
                        softDeletable.DeletedDate = now;
                    }

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

    private sealed class Resolver(DbContext context, List<EntityEntry> deleted, HashSet<object>? owners)
    {
        private readonly HashSet<object> _resolving = new(ReferenceEqualityComparer.Instance);

        public Dictionary<object, Outcome> Outcomes { get; } = new(ReferenceEqualityComparer.Instance);

        // Null while the entry is being decided: a principal reached again through a cascade cycle does not count.
        public Outcome? Resolve(EntityEntry entry)
        {
            if (Outcomes.TryGetValue(entry.Entity, out var known))
                return known;

            if (!_resolving.Add(entry.Entity))
                return null;

            var outcome = Decide(entry);
            _resolving.Remove(entry.Entity);
            Outcomes[entry.Entity] = outcome;
            return outcome;
        }

        private Outcome Decide(EntityEntry entry)
        {
            if (owners?.Contains(entry.Entity) == true)
                return Outcome.SoftDelete;

            if (PermanentDeletes.IsMarked(context, entry.Entity))
                return Outcome.Delete;

            var principals = CascadingPrincipals(entry)
                .Select(Resolve)
                .OfType<Outcome>()
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

        // Only relationships whose delete removes the dependent: a SetNull or Restrict principal does not take it along.
        private IEnumerable<EntityEntry> CascadingPrincipals(EntityEntry dependent) =>
            dependent.Metadata.GetForeignKeys()
                .Where(foreignKey => foreignKey.DeleteBehavior is DeleteBehavior.Cascade or DeleteBehavior.ClientCascade)
                .SelectMany(foreignKey => deleted.Where(principal =>
                    foreignKey.PrincipalEntityType.IsAssignableFrom(principal.Metadata)
                    && ReferencesPrincipal(dependent, principal, foreignKey)));
    }

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
