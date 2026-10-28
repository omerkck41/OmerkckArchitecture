using System.Data.Common;
using System.Runtime.CompilerServices;
using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// EF Core interceptor that collects and dispatches domain events after SaveChanges.
/// Events saved inside an explicit transaction wait for that transaction: they are dropped when it rolls back, fails or
/// is disposed uncommitted, and are dispatched after a successful commit — by <c>IUnitOfWork</c> right after the
/// commit, or by the context's next SaveChanges when the transaction was committed directly.
/// If the dispatcher throws, the events go back to their entities so the next save dispatches them again.
/// </summary>
public sealed partial class DomainEventDispatchInterceptor(
    IDomainEventDispatcher dispatcher,
    ILogger<DomainEventDispatchInterceptor> logger) : SaveChangesInterceptor, IDbTransactionInterceptor
{
    private readonly ConditionalWeakTable<DbTransaction, List<EntityEvents>> _parked = new();
    private readonly ConditionalWeakTable<DbContext, List<EntityEvents>> _committed = new();

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await OnSavedAsync(eventData.Context, cancellationToken);

        return result;
    }

    /// <inheritdoc />
    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is not null)
        {
            LogSyncSaveChanges(logger);
            Task.Run(() => OnSavedAsync(eventData.Context, CancellationToken.None)).GetAwaiter().GetResult();
        }

        return result;
    }

    // A provider may hand out the same DbTransaction object again (Npgsql reuses it per connector), so a transaction
    // that was disposed uncommitted must not leave its events attached to the next one.

    /// <inheritdoc />
    public DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        _parked.Remove(result);
        return result;
    }

    /// <inheritdoc />
    public ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
    {
        _parked.Remove(result);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public DbTransaction TransactionUsed(DbConnection connection, TransactionEventData eventData, DbTransaction result)
    {
        _parked.Remove(result);
        return result;
    }

    /// <inheritdoc />
    public ValueTask<DbTransaction> TransactionUsedAsync(
        DbConnection connection,
        TransactionEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
    {
        _parked.Remove(result);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        TransactionCommitted(transaction, eventData);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        // EF still holds the transaction here, so a handler saving through the same context would fail;
        // the events are dispatched once the commit has fully returned.
        if (eventData.Context is null || !_parked.TryGetValue(transaction, out var parked))
            return;

        _parked.Remove(transaction);
        _committed.GetValue(eventData.Context, _ => []).AddRange(parked);
    }

    /// <inheritdoc />
    public Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _parked.Remove(transaction);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) =>
        _parked.Remove(transaction);

    /// <inheritdoc />
    public Task TransactionFailedAsync(
        DbTransaction transaction,
        TransactionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _parked.Remove(transaction);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData) =>
        _parked.Remove(transaction);

    /// <summary>Dispatches the events of transactions this context has committed.</summary>
    internal Task DispatchCommittedAsync(DbContext context, CancellationToken cancellationToken) =>
        DispatchAsync(TakeCommitted(context), cancellationToken);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sync SavedChanges triggered domain event dispatch. Prefer SaveChangesAsync to avoid thread pool offloading")]
    private static partial void LogSyncSaveChanges(ILogger logger);

    private async Task OnSavedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var collected = CollectAndClear(context);

        if ((context.Database.CurrentTransaction as IInfrastructure<DbTransaction>)?.Instance is { } transaction)
        {
            if (collected.Count > 0)
                _parked.GetValue(transaction, _ => []).AddRange(collected);
            return;
        }

        var pending = TakeCommitted(context);
        pending.AddRange(collected);
        await DispatchAsync(pending, cancellationToken);
    }

    private List<EntityEvents> TakeCommitted(DbContext context)
    {
        if (!_committed.TryGetValue(context, out var committed))
            return [];

        _committed.Remove(context);
        return committed;
    }

    private async Task DispatchAsync(List<EntityEvents> collected, CancellationToken cancellationToken)
    {
        if (collected.Count == 0)
            return;

        try
        {
            await dispatcher.DispatchAsync(collected.SelectMany(c => c.Events).ToList(), cancellationToken);
        }
        catch
        {
            foreach (var (entity, events) in collected)
            {
                foreach (var domainEvent in events)
                    Invoke(entity, nameof(Entity<object>.AddDomainEvent), [typeof(IDomainEvent)], domainEvent);
            }

            throw;
        }
    }

    private static List<EntityEvents> CollectAndClear(DbContext context)
    {
        var collected = new List<EntityEvents>();

        foreach (var entity in context.ChangeTracker.Entries().Select(e => e.Entity))
        {
            if (entity.GetType().GetProperty(nameof(Entity<object>.DomainEvents))?.GetValue(entity)
                    is not IReadOnlyCollection<IDomainEvent> { Count: > 0 } events)
                continue;

            collected.Add(new EntityEvents(entity, events.ToList()));
            Invoke(entity, nameof(Entity<object>.ClearDomainEvents), Type.EmptyTypes);
        }

        return collected;
    }

    private static void Invoke(object entity, string method, Type[] parameterTypes, params object[] arguments) =>
        entity.GetType().GetMethod(method, parameterTypes)?.Invoke(entity, arguments);

    private sealed record EntityEvents(object Entity, IReadOnlyList<IDomainEvent> Events);
}
