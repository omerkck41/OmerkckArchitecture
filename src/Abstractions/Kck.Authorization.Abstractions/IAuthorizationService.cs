namespace Kck.Authorization.Abstractions;

/// <summary>
/// Authorizes the current user against required roles and claims.
/// Throws if authorization fails.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Ensures the current user has at least one of the required roles.
    /// Throws UnauthorizedException or ForbiddenException on failure.
    /// </summary>
    Task AuthorizeAsync(string[] requiredRoles, CancellationToken ct = default);

    /// <summary>
    /// Ensures the current user has the required claims.
    /// </summary>
    Task AuthorizeAsync(IReadOnlyDictionary<string, string> requiredClaims, CancellationToken ct = default);

    /// <summary>
    /// Ensures the current user has the required roles AND claims.
    /// </summary>
    Task AuthorizeAsync(string[] requiredRoles, IReadOnlyDictionary<string, string> requiredClaims, CancellationToken ct = default);
}
