namespace Kck.Core.Abstractions.Pipeline;

/// <summary>
/// Marks a pipeline request for structured logging.
/// </summary>
/// <remarks>
/// Migrate to <see cref="Kck.Pipeline.Abstractions.ILoggableRequest"/> before v3.1.
/// </remarks>
[Obsolete("Use Kck.Pipeline.Abstractions.ILoggableRequest. This type will be removed in v3.1.")]
public interface ILoggableRequest : Kck.Pipeline.Abstractions.ILoggableRequest { }
