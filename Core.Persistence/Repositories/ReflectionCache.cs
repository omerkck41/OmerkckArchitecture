using System.Linq.Expressions;
using System.Reflection;

namespace Core.Persistence.Repositories;

public static class ReflectionDelegateCache
{
    private static readonly Dictionary<Type, (Func<object, object?>? GetIsDeleted, Action<object, object?>? SetIsDeleted,
        Func<object, object?>? GetDeletedDate, Action<object, object?>? SetDeletedDate,
        Func<object, object?>? GetDeletedBy, Action<object, object?>? SetDeletedBy)> _cache = new();

    public static (Func<object, object?>? GetIsDeleted, Action<object, object?>? SetIsDeleted,
        Func<object, object?>? GetDeletedDate, Action<object, object?>? SetDeletedDate,
        Func<object, object?>? GetDeletedBy, Action<object, object?>? SetDeletedBy) GetDelegates(Type type)
    {
        if (!_cache.TryGetValue(type, out var delegates))
        {
            var isDeletedProp = type.GetProperty("IsDeleted");
            var deletedDateProp = type.GetProperty("DeletedDate");
            var deletedByProp = type.GetProperty("DeletedBy");

            var getIsDeleted = isDeletedProp != null ? CreateGetter(isDeletedProp) : null;
            var setIsDeleted = isDeletedProp != null ? CreateSetter(isDeletedProp) : null;
            var getDeletedDate = deletedDateProp != null ? CreateGetter(deletedDateProp) : null;
            var setDeletedDate = deletedDateProp != null ? CreateSetter(deletedDateProp) : null;
            var getDeletedBy = deletedByProp != null ? CreateGetter(deletedByProp) : null;
            var setDeletedBy = deletedByProp != null ? CreateSetter(deletedByProp) : null;

            delegates = (getIsDeleted, setIsDeleted, getDeletedDate, setDeletedDate, getDeletedBy, setDeletedBy);
            _cache[type] = delegates;
        }
        return delegates;
    }

    private static Func<object, object?> CreateGetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var convert = Expression.Convert(instance, property.DeclaringType);
        var propertyAccess = Expression.Property(convert, property);
        var convertResult = Expression.Convert(propertyAccess, typeof(object));
        var lambda = Expression.Lambda<Func<object, object?>>(convertResult, instance);
        return lambda.Compile();
    }

    private static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var convertInstance = Expression.Convert(instance, property.DeclaringType);
        var convertValue = Expression.Convert(value, property.PropertyType);
        var propertyAccess = Expression.Property(convertInstance, property);
        var assign = Expression.Assign(propertyAccess, convertValue);
        var lambda = Expression.Lambda<Action<object, object?>>(assign, instance, value);
        return lambda.Compile();
    }
}