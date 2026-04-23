namespace Kck.Authorization.Abstractions;

/// <summary>
/// Checks whether the current user has specific permissions.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>Checks if the current user has the specified permission.</summary>
    Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default);

    /// <summary>Checks if the current user has ALL of the specified permissions.</summary>
    Task<bool> HasAllPermissionsAsync(IEnumerable<string> permissions, CancellationToken ct = default);

    /// <summary>Checks if the current user has ANY of the specified permissions.</summary>
    Task<bool> HasAnyPermissionAsync(IEnumerable<string> permissions, CancellationToken ct = default);
}
