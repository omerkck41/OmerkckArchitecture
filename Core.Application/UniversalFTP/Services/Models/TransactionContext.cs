namespace Core.Application.UniversalFTP.Services.Models;

public class TransactionContext
{
    private readonly List<Func<Task>> _operations = new();
    private readonly List<Func<Task>> _rollbackOperations = new();
    private readonly List<Func<Task>> _rollbackActions = new();

    public void AddRollbackAction(Func<Task> rollbackAction)
    {
        _rollbackActions.Add(rollbackAction);
    }

    public void AddOperation(Func<Task> operation, Func<Task> rollbackOperation)
    {
        _operations.Add(operation);
        _rollbackOperations.Add(rollbackOperation);
    }

    public async Task CommitAsync()
    {
        foreach (var operation in _operations)
        {
            await operation();
        }
    }

    public async Task RollbackAsync()
    {
        foreach (var action in _rollbackActions)
        {
            await action();
        }
    }

    public void Dispose()
    {
        _rollbackActions.Clear();
    }
}
