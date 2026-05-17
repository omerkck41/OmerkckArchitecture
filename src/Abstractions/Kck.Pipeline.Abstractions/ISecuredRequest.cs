namespace Kck.Pipeline.Abstractions;

/// <summary>
/// Marks a pipeline request that requires role-based authorization.
/// <c>AuthorizationBehavior</c> enforces that the current user holds at least one of <see cref="Roles"/>.
/// </summary>
public interface ISecuredRequest
{
    /// <summary>Required roles. Any single matching role grants access.</summary>
    string[] Roles { get; }
}
