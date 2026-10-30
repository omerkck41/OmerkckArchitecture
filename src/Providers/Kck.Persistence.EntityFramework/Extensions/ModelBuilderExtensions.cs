using System.Linq.Expressions;
using System.Reflection;
using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kck.Persistence.EntityFramework.Extensions;

/// <summary>
/// Extension methods for <see cref="ModelBuilder"/> to apply global conventions.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Name of the EF Core query filter added by <see cref="ApplyGlobalSoftDeleteQueryFilter"/>. Repository queries
    /// with <c>QueryOptions.IncludeDeleted</c> lift only this filter; pass it to
    /// <c>IgnoreQueryFilters([SoftDeleteQueryFilterName])</c> to do the same in your own queries.
    /// </summary>
    public const string SoftDeleteQueryFilterName = "KckSoftDelete";

    private static readonly MethodInfo ApplyFilterMethod =
        typeof(ModelBuilderExtensions).GetMethod(
            nameof(ApplySoftDeleteFilter),
            BindingFlags.NonPublic | BindingFlags.Static)!; // NOSONAR S3011 - NonPublic reflection is intentional: EF Core global filter requires generic method invocation via reflection

    /// <summary>
    /// Applies a global query filter, named <see cref="SoftDeleteQueryFilterName"/>, that excludes soft-deleted
    /// entities for all entity types implementing <see cref="ISoftDeletable"/>. Other filters on those types must be
    /// named too (EF Core cannot combine unnamed and named filters); a named filter such as a tenant filter keeps
    /// applying when a query includes deleted rows. Call it last in <c>OnModelCreating</c>: it covers the entity types
    /// in the model at that point.
    /// </summary>
    /// <exception cref="InvalidOperationException">A soft-deletable entity type already has an unnamed query filter, is
    /// owned, or derives from a root type that is not soft-deletable (no filter could hide its deleted rows).</exception>
    public static ModelBuilder ApplyGlobalSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        var softDeletables = modelBuilder.Model.GetEntityTypes()
            .Where(et => typeof(ISoftDeletable).IsAssignableFrom(et.ClrType))
            .ToList();

        // Before 4.6 this method replaced such a filter without a trace; failing here keeps, e.g., a tenant filter.
        var unnamed = softDeletables.FirstOrDefault(et => et.GetDeclaredQueryFilters().Any(f => f.IsAnonymous));
        if (unnamed is not null)
        {
            throw new InvalidOperationException(
                $"Entity type '{unnamed.DisplayName()}' has an unnamed query filter. ApplyGlobalSoftDeleteQueryFilter adds " +
                $"the named filter '{SoftDeleteQueryFilterName}', and EF Core cannot combine unnamed and named filters. " +
                "Give your filter a name, for example HasQueryFilter(\"Tenant\", e => ...).");
        }

        // EF Core accepts query filters only on the root of a hierarchy (derived types inherit it) and not on owned types.
        // Where no filter can hide the soft-deleted rows, fail instead of leaving them visible.
        var owned = softDeletables.FirstOrDefault(et => et.IsOwned());
        if (owned is not null)
        {
            throw new InvalidOperationException(
                $"Entity type '{owned.DisplayName()}' is owned and implements ISoftDeletable. EF Core cannot filter owned " +
                "types, so its soft-deleted rows would stay visible. Map it as a regular entity, or remove ISoftDeletable.");
        }

        var unfilterable = softDeletables.FirstOrDefault(et => et.BaseType is not null
            && !typeof(ISoftDeletable).IsAssignableFrom(et.GetRootType().ClrType));
        if (unfilterable is not null)
        {
            throw new InvalidOperationException(
                $"Entity type '{unfilterable.DisplayName()}' implements ISoftDeletable but the root of its hierarchy, " +
                $"'{unfilterable.GetRootType().DisplayName()}', does not. EF Core filters only root types, so its " +
                "soft-deleted rows would stay visible. Implement ISoftDeletable on the root type.");
        }

        foreach (var entityType in softDeletables.Where(et => et.BaseType is null))
        {
            var method = ApplyFilterMethod.MakeGenericMethod(entityType.ClrType);
            method.Invoke(null, [modelBuilder]);
        }

        return modelBuilder;
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : class, ISoftDeletable
    {
        Expression<Func<T, bool>> filter = e => !e.IsDeleted;
        modelBuilder.Entity<T>().HasQueryFilter(SoftDeleteQueryFilterName, filter);
    }
}
