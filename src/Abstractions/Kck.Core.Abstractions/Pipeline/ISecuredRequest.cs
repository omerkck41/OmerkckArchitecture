namespace Kck.Core.Abstractions.Pipeline;

/// <summary>
/// Marks a pipeline request that requires role-based authorization.
/// </summary>
/// <remarks>
/// Migrate to <see cref="Kck.Pipeline.Abstractions.ISecuredRequest"/> before v3.1.
/// </remarks>
[Obsolete("Use Kck.Pipeline.Abstractions.ISecuredRequest. This type will be removed in v3.1.")]
public interface ISecuredRequest : Kck.Pipeline.Abstractions.ISecuredRequest { }
