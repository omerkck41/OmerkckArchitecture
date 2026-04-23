namespace Kck.FeatureFlags.Abstractions;

public interface IFeatureContext
{
    string? UserId { get; }
    string? TenantId { get; }
    IReadOnlyDictionary<string, string> Properties { get; }
}
