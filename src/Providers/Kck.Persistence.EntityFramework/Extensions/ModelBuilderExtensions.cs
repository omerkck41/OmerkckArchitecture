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
    /// <exception cref="InvalidOperationException">A soft-deletable entity type already has an unnamed query filter.</exception>
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

        foreach (var entityType in softDeletables)
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
