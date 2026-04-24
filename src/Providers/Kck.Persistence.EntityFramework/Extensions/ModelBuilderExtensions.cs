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
    private static readonly MethodInfo ApplyFilterMethod =
        typeof(ModelBuilderExtensions).GetMethod(
            nameof(ApplySoftDeleteFilter),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    /// Applies a global query filter to exclude soft-deleted entities
    /// for all entity types implementing <see cref="ISoftDeletable"/>.
    /// </summary>
    public static ModelBuilder ApplyGlobalSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        var softDeletables = modelBuilder.Model.GetEntityTypes()
            .Where(et => typeof(ISoftDeletable).IsAssignableFrom(et.ClrType));

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
        modelBuilder.Entity<T>().HasQueryFilter(filter);
    }
}
