namespace Kck.Core.Abstractions.Pipeline;

/// <summary>
/// Marks a pipeline request that must execute within a database transaction.
/// </summary>
/// <remarks>
/// Migrate to <see cref="Kck.Pipeline.Abstractions.ITransactionalRequest"/> before v3.1.
/// </remarks>
[Obsolete("Use Kck.Pipeline.Abstractions.ITransactionalRequest. This type will be removed in v3.1.")]
public interface ITransactionalRequest : Kck.Pipeline.Abstractions.ITransactionalRequest { }
