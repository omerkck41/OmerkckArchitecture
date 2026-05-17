namespace Kck.Persistence.Abstractions.Security;

/// <summary>
/// Marks an entity property as eligible for use in dynamic filter expressions.
/// Properties without this attribute are rejected by the filter security layer, preventing unauthorized data probing.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FilterableAttribute : Attribute;
