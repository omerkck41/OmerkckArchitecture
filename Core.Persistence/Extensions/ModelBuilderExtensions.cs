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
            if (clrType == null) continue;

            // Eğer Entity Generic ise (örneğin OperationClaim<int> gibi) atla
            if (clrType.IsGenericType || clrType.BaseType?.IsGenericType == true)
            {
                Console.WriteLine($"[DEBUG] Query Filter EKLENMEDİ (Generic): {clrType.Name}");
                continue;
            }

            var isDeletedProperty = clrType.GetProperty("IsDeleted", BindingFlags.Public | BindingFlags.Instance);
            if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
            {
                Console.WriteLine($"[DEBUG] Query Filter Ekleniyor: {clrType.Name}");

                var entity = modelBuilder.Entity(clrType);

                var parameter = Expression.Parameter(clrType, "e");
                var propertyAccess = Expression.Property(parameter, "IsDeleted");
                var compareExpression = Expression.Equal(propertyAccess, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                entity.HasQueryFilter(lambda);
            }
        }
    }
}