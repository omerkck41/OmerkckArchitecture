namespace Kck.Core.Abstractions.Pipeline;

/// <summary>
/// Marks a pipeline request whose response should be cached.
/// </summary>
/// <remarks>
/// This type is retained for backwards compatibility. Migrate to
/// <see cref="Kck.Pipeline.Abstractions.ICachableRequest"/> before v3.1.
/// </remarks>
[Obsolete("Use Kck.Pipeline.Abstractions.ICachableRequest. This type will be removed in v3.1.")]
public interface ICachableRequest : Kck.Pipeline.Abstractions.ICachableRequest { }
