namespace Kck.Pipeline.Abstractions;

/// <summary>
/// Marks a pipeline request that must execute within a database transaction.
/// <c>TransactionBehavior</c> wraps the handler in a unit-of-work scope.
/// </summary>
public interface ITransactionalRequest;
