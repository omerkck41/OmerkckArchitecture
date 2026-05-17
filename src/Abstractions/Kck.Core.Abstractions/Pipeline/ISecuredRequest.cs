namespace Kck.Core.Abstractions.Pipeline;

/// <summary>Marks a pipeline request that requires role-based authorization enforced by <c>AuthorizationBehavior</c>.</summary>
public interface ISecuredRequest
{
    /// <summary>Required roles for this request; authorization passes if the caller holds at least one of the listed roles.</summary>
    string[] Roles { get; }
}
