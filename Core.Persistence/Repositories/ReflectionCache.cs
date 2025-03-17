using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Persistence.Repositories;

public static class ReflectionDelegateCache
{
    private static readonly ConcurrentDictionary<Type, (Func<object, object?>? GetIsDeleted, Action<object, object?>? SetIsDeleted,
           Func<object, object?>? GetDeletedDate, Action<object, object?>? SetDeletedDate,
           Func<object, object?>? GetDeletedBy, Action<object, object?>? SetDeletedBy)> _cache =
           new ConcurrentDictionary<Type, (Func<object, object?>?, Action<object, object?>?, Func<object, object?>?, Action<object, object?>?, Func<object, object?>?, Action<object, object?>?)>();

    public static (Func<object, object?>? GetIsDeleted, Action<object, object?>? SetIsDeleted,
        Func<object, object?>? GetDeletedDate, Action<object, object?>? SetDeletedDate,
        Func<object, object?>? GetDeletedBy, Action<object, object?>? SetDeletedBy) GetDelegates(Type type)
    {
        return _cache.GetOrAdd(type, t =>
        {
            var isDeletedProp = t.GetProperty("IsDeleted");
            var deletedDateProp = t.GetProperty("DeletedDate");
            var deletedByProp = t.GetProperty("DeletedBy");

            var getIsDeleted = isDeletedProp != null ? CreateGetter(isDeletedProp) : null;
            var setIsDeleted = isDeletedProp != null ? CreateSetter(isDeletedProp) : null;
            var getDeletedDate = deletedDateProp != null ? CreateGetter(deletedDateProp) : null;
            var setDeletedDate = deletedDateProp != null ? CreateSetter(deletedDateProp) : null;
            var getDeletedBy = deletedByProp != null ? CreateGetter(deletedByProp) : null;
            var setDeletedBy = deletedByProp != null ? CreateSetter(deletedByProp) : null;

            return (getIsDeleted, setIsDeleted, getDeletedDate, setDeletedDate, getDeletedBy, setDeletedBy);
        });
    }

    private static Func<object, object?> CreateGetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var convert = Expression.Convert(instance, property.DeclaringType!);
        var propertyAccess = Expression.Property(convert, property);
        var convertResult = Expression.Convert(propertyAccess, typeof(object));
        var lambda = Expression.Lambda<Func<object, object?>>(convertResult, instance);
        return lambda.Compile();
    }

    private static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var convertInstance = Expression.Convert(instance, property.DeclaringType!);
        var convertValue = Expression.Convert(value, property.PropertyType);
        var propertyAccess = Expression.Property(convertInstance, property);
        var assign = Expression.Assign(propertyAccess, convertValue);
        var lambda = Expression.Lambda<Action<object, object?>>(assign, instance, value);
        return lambda.Compile();
    }
}