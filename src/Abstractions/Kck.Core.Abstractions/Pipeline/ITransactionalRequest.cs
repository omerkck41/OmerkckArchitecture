namespace Kck.Core.Abstractions.Pipeline;

/// <summary>Marks a pipeline request that must execute within a database transaction wrapped by <c>TransactionBehavior</c>.</summary>
public interface ITransactionalRequest;
