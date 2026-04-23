namespace Kck.Persistence.Abstractions.Security;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FilterableAttribute : Attribute;
