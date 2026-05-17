using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Kck.Persistence.Abstractions.Security;

/// <summary>
/// Maintains the set of properties on <typeparamref name="T"/> that may be used in dynamic filter queries.
/// </summary>
public class FilterPropertyWhitelist<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IFilterPropertyWhitelist<T>
{
    private readonly HashSet<string> _allowed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The case-insensitive set of property names currently on the allow-list.</summary>
    public IReadOnlySet<string> AllowedProperties => _allowed;

    /// <summary>Initializes the whitelist and auto-populates it from <see cref="FilterableAttribute"/> annotations on <typeparamref name="T"/>.</summary>
    public FilterPropertyWhitelist()
    {
        ScanFilterableAttributes();
    }

    /// <summary>Returns <c>true</c> if the property name is on the allow-list.</summary>
    public bool IsAllowed(string propertyName) => _allowed.Contains(propertyName);

    /// <summary>Adds the specified property to the allow-list and returns this instance for fluent chaining.</summary>
    protected FilterPropertyWhitelist<T> Allow(Expression<Func<T, object>> property)
    {
        var memberName = GetMemberName(property);
        _allowed.Add(memberName);
        return this;
    }

    /// <summary>Removes the specified property from the allow-list and returns this instance for fluent chaining.</summary>
    protected FilterPropertyWhitelist<T> Deny(Expression<Func<T, object>> property)
    {
        var memberName = GetMemberName(property);
        _allowed.Remove(memberName);
        return this;
    }

    private void ScanFilterableAttributes()
    {
        var filterable = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<FilterableAttribute>() is not null);

        foreach (var prop in filterable)
            _allowed.Add(prop.Name);
    }

    private static string GetMemberName(Expression<Func<T, object>> expression)
    {
        var body = expression.Body;

        if (body is UnaryExpression { Operand: MemberExpression unaryMember })
            return unaryMember.Member.Name;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException("Expression must be a property access expression.");
    }
}
