namespace Kck.Persistence.Abstractions.Security;

public interface IFilterPropertyWhitelist<T>
{
    IReadOnlySet<string> AllowedProperties { get; }
    bool IsAllowed(string propertyName);
}
