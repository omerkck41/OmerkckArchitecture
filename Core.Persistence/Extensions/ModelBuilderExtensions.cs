using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyGlobalSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isDeletedProperty = clrType.GetProperty("IsDeleted", BindingFlags.Public | BindingFlags.Instance);
            if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
            {
                // Parametre: e =>
                var parameter = Expression.Parameter(clrType, "e");

                // Expression: EF.Property<bool>(e, "IsDeleted")
                var propertyMethodInfo = typeof(EF)
                    .GetMethod("Property", BindingFlags.Static | BindingFlags.Public)
                    ?.MakeGenericMethod(typeof(bool));
                if (propertyMethodInfo == null)
                    continue;

                var propertyAccess = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));

                // Expression: EF.Property<bool>(e, "IsDeleted") == false
                var compareExpression = Expression.Equal(propertyAccess, Expression.Constant(false));

                // Lambda: e => EF.Property<bool>(e, "IsDeleted") == false
                var lambda = Expression.Lambda(compareExpression, parameter);

                // Global query filter ekle
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }
}