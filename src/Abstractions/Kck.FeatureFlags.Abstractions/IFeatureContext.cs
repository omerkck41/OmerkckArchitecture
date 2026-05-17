namespace Kck.FeatureFlags.Abstractions;

/// <summary>
/// Provides caller-supplied contextual data (user, tenant, and arbitrary properties) used by feature evaluators to make per-request flag decisions.
/// </summary>
public interface IFeatureContext
{
    /// <summary>Gets the identifier of the current user, or <see langword="null"/> for anonymous contexts.</summary>
    string? UserId { get; }

    /// <summary>Gets the identifier of the current tenant, or <see langword="null"/> when multi-tenancy is not applicable.</summary>
    string? TenantId { get; }

    /// <summary>Gets a dictionary of additional context properties that custom evaluators may inspect.</summary>
    IReadOnlyDictionary<string, string> Properties { get; }
}
