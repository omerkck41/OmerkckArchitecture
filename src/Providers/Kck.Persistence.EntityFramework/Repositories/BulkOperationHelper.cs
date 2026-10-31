using System.Linq.Expressions;
using System.Reflection;
using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Kck.Persistence.EntityFramework.Repositories;

/// <summary>
/// Encapsulates bulk update operations that bypass the change tracker.
/// </summary>
internal static class BulkOperationHelper
{
    /// <summary>
    /// Performs a bulk update using EF Core's ExecuteUpdateAsync (single SQL statement).
    /// <para>
    /// WARNING: Bypasses change tracker and interceptors (e.g., audit).
    /// </para>
    /// <para>
    /// Each value must match the property's CLR type exactly (no numeric widening); it is passed to
    /// the typed <c>SetProperty&lt;TProperty&gt;</c> so every provider can assign a parameter type mapping.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentException">A value is not assignable to its property type.</exception>
    public static async Task BulkUpdateAsync<T, TId>(
        IQueryable<T> rows,
        (Expression<Func<T, object>> Property, object? Value)[] updates,
        CancellationToken cancellationToken = default)
        where T : Entity<TId>
    {
        // Validate everything before touching the database so a bad entry never yields a partial update.
        var typedUpdates = updates.Select(ToTypedUpdate).ToArray();

        await rows
            .ExecuteUpdateAsync(setters =>
            {
                foreach (var (property, propertyType, value) in typedUpdates)
                    TypedSetter<T>.SetPropertyValue.MakeGenericMethod(propertyType).Invoke(setters, [property, value]);
            }, cancellationToken).ConfigureAwait(false);
    }

    private static (LambdaExpression Property, Type PropertyType, object? Value) ToTypedUpdate<T>(
        (Expression<Func<T, object>> Property, object? Value) update)
    {
        // Value-type properties arrive boxed as Convert(e.Prop, object); unwrap to recover the real CLR type.
        var body = update.Property.Body is UnaryExpression { NodeType: ExpressionType.Convert } convert
            ? convert.Operand
            : update.Property.Body;
        var propertyType = body.Type;

        if (update.Value is null)
        {
            if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) is null)
                throw new ArgumentException(
                    $"Cannot assign null to property '{body}' of non-nullable type '{propertyType.Name}'.",
                    nameof(update));
        }
        else if (!propertyType.IsInstanceOfType(update.Value))
        {
            throw new ArgumentException(
                $"Value for property '{body}' must be of type '{propertyType.Name}' but was '{update.Value.GetType().Name}'.",
                nameof(update));
        }

        return (Expression.Lambda(body, update.Property.Parameters), propertyType, update.Value);
    }

    private static class TypedSetter<T>
    {
        // UpdateSettersBuilder<T>.SetProperty<TProperty>(Expression<Func<T, TProperty>>, TProperty value)
        public static readonly MethodInfo SetPropertyValue = typeof(UpdateSettersBuilder<T>).GetMethods()
            .Single(m => m.Name == nameof(UpdateSettersBuilder<T>.SetProperty)
                && m.GetParameters()[1].ParameterType.IsGenericParameter);
    }
}
