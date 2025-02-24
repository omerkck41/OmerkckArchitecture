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

            var method = typeof(ModelBuilder)
                .GetMethods()
                .FirstOrDefault(m => m.Name == nameof(ModelBuilder.Entity) && m.GetParameters().Length == 0)?
                .MakeGenericMethod(clrType);

            if (method == null) continue;

            var entityTypeBuilder = method.Invoke(modelBuilder, null);
            var hasQueryFilterMethod = entityTypeBuilder.GetType().GetMethod("HasQueryFilter");

            if (hasQueryFilterMethod != null)
            {
                var parameter = Expression.Parameter(clrType, "e");

                // EF.Property<bool>(e, "IsDeleted")
                var propertyMethodInfo = typeof(EF)
                    .GetMethod("Property", BindingFlags.Static | BindingFlags.Public)?
                    .MakeGenericMethod(typeof(bool));

                if (propertyMethodInfo == null) continue;

                var propertyAccess = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));

                // EF.Property<bool>(e, "IsDeleted") == false
                var compareExpression = Expression.Equal(propertyAccess, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                hasQueryFilterMethod.Invoke(entityTypeBuilder, new object[] { lambda });
            }
        }
    }
}