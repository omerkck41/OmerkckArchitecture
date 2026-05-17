namespace Kck.Persistence.Abstractions.Security;

/// <summary>
/// Defines the contract for a per-entity property whitelist used by the dynamic filter security layer.
/// Implementations declare which properties of <typeparamref name="T"/> may be referenced in client-supplied filters.
/// </summary>
/// <typeparam name="T">The entity type whose filterable properties are being governed.</typeparam>
public interface IFilterPropertyWhitelist<T>
{
    /// <summary>
    /// The set of property names that are permitted in dynamic filter expressions for <typeparamref name="T"/>.
    /// </summary>
    IReadOnlySet<string> AllowedProperties { get; }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="propertyName"/> is in the allowed set; otherwise <c>false</c>.
    /// </summary>
    /// <param name="propertyName">The property name to check.</param>
    bool IsAllowed(string propertyName);
}
