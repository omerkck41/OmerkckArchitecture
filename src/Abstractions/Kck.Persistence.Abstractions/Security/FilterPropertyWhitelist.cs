using System.Linq.Expressions;
using System.Reflection;

namespace Kck.Persistence.Abstractions.Security;

public class FilterPropertyWhitelist<T> : IFilterPropertyWhitelist<T>
{
    private readonly HashSet<string> _allowed = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> AllowedProperties => _allowed;

    public FilterPropertyWhitelist()
    {
        ScanFilterableAttributes();
    }

    public bool IsAllowed(string propertyName) => _allowed.Contains(propertyName);

    protected FilterPropertyWhitelist<T> Allow(Expression<Func<T, object>> property)
    {
        var memberName = GetMemberName(property);
        _allowed.Add(memberName);
        return this;
    }

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
