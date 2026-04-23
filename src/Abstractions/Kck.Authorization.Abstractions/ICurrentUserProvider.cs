namespace Kck.Authorization.Abstractions;

/// <summary>
/// Provides information about the currently authenticated user.
/// Typically backed by HttpContext.User claims.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>The user's unique identifier. Null if not authenticated.</summary>
    string? UserId { get; }

    /// <summary>The user's email address.</summary>
    string? Email { get; }

    /// <summary>The user's display name.</summary>
    string? Name { get; }

    /// <summary>Whether the user is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>The user's assigned roles.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>All claims for the current user.</summary>
    IReadOnlyDictionary<string, string> Claims { get; }

    /// <summary>Checks if the user has a specific role.</summary>
    bool IsInRole(string role);
}
