using System.Data.Common;
using System.Runtime.CompilerServices;
using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// EF Core interceptor that collects and dispatches domain events after SaveChanges.
/// Events saved inside an explicit transaction wait for that transaction: they are dropped when it rolls back, fails or
/// is disposed uncommitted, and are dispatched after a successful commit — by <c>IUnitOfWork</c> right after the
/// commit, or by the context's next SaveChanges when the transaction was committed directly.
/// If the dispatcher throws, the events go back to their entities so the next save dispatches them again.
/// </summary>
public sealed partial class DomainEventDispatchInterceptor : SaveChangesInterceptor, IDbTransactionInterceptor
{
    private readonly IDomainEventDispatcher? _dispatcher;
    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly ILogger<DomainEventDispatchInterceptor> logger;
    private readonly ConditionalWeakTable<DbTransaction, List<EntityEvents>> _parked = new();
    private readonly ConditionalWeakTable<DbContext, List<EntityEvents>> _committed = new();
    private readonly ConditionalWeakTable<DbContext, List<EntityEvents>> _saving = new();
    private readonly ConditionalWeakTable<DbContext, StrongBox<int>> _leases = new();

    /// <summary>Dispatches through <paramref name="dispatcher"/> (a scoped interceptor, one per DbContext scope).</summary>
    public DomainEventDispatchInterceptor(IDomainEventDispatcher dispatcher, ILogger<DomainEventDispatchInterceptor> logger)
    {
        _dispatcher = dispatcher;
        this.logger = logger;
    }

    // Pooled contexts share one options instance, so the interceptor is a singleton: each dispatch resolves the
    // dispatcher from a new scope. Internal so the container never has to choose between two constructors.
    internal DomainEventDispatchInterceptor(IServiceScopeFactory scopeFactory, ILogger<DomainEventDispatchInterceptor> logger)
    {
        _scopeFactory = scopeFactory;
        this.logger = logger;
    }

    // Events are collected before the save: once it succeeds, EF detaches deleted entities, and their events would be
    // out of reach.

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(SavingChanges(eventData, result));

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return result;

        ForgetPreviousLease(eventData.Context);

        // A save that failed before EF reports SaveChangesFailed (an interceptor after this one threw) leaves its
        // events here; put them back so only entities still tracked contribute them again.
        Restore(Take(_saving, eventData.Context));
        _saving.GetValue(eventData.Context, _ => []).AddRange(CollectAndClear(eventData.Context));
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
        ConcurrencyExceptionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(ThrowingConcurrencyException(eventData, result));

    /// <inheritdoc />
    public override InterceptionResult ThrowingConcurrencyException(
        ConcurrencyExceptionEventData eventData,
        InterceptionResult result)
    {
        if (eventData.Context is not null)
            Restore(Take(_saving, eventData.Context));

        return result;
    }

    /// <inheritdoc />
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        SaveChangesFailed(eventData);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
            Restore(Take(_saving, eventData.Context));
    }

    /// <inheritdoc />
    public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        SaveChangesCanceled(eventData);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void SaveChangesCanceled(DbContextEventData eventData)
    {
        if (eventData.Context is not null)
            Restore(Take(_saving, eventData.Context));
    }

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
        ForgetPreviousLease(eventData.Context);
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
    internal Task DispatchCommittedAsync(DbContext context, CancellationToken cancellationToken)
    {
        ForgetPreviousLease(context);
        return DispatchAsync(Take(_committed, context), cancellationToken);
    }

    // A pooled DbContext serves one request per lease. Events a lease left waiting (a transaction committed directly
    // with no SaveChanges after it) belong to that request: they end with it, as they would with a non-pooled context
    // and its scope, instead of being dispatched in the next request. Non-pooled contexts keep lease 0.
    private void ForgetPreviousLease(DbContext context)
    {
        var lease = context.ContextId.Lease;
        var seen = _leases.GetValue(context, _ => new StrongBox<int>(lease));
        if (seen.Value == lease)
            return;

        seen.Value = lease;
        var dropped = Take(_committed, context).Count + Take(_saving, context).Count;
        if (dropped > 0)
            LogPreviousLeaseEventsDropped(logger);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Domain events left by a previous lease of a pooled DbContext were dropped; commit transactions through IUnitOfWork")]
    private static partial void LogPreviousLeaseEventsDropped(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sync SavedChanges triggered domain event dispatch. Prefer SaveChangesAsync to avoid thread pool offloading")]
    private static partial void LogSyncSaveChanges(ILogger logger);

    private async Task OnSavedAsync(DbContext context, CancellationToken cancellationToken)
    {
        ForgetPreviousLease(context);

        // Events raised during the save (after this interceptor's SavingChanges) are still on tracked entities.
        var collected = Take(_saving, context);
        collected.AddRange(CollectAndClear(context));

        if ((context.Database.CurrentTransaction as IInfrastructure<DbTransaction>)?.Instance is { } transaction)
        {
            if (collected.Count > 0)
                _parked.GetValue(transaction, _ => []).AddRange(collected);
            return;
        }

        var pending = Take(_committed, context);
        pending.AddRange(collected);
        await DispatchAsync(pending, cancellationToken);
    }

    private static List<EntityEvents> Take(ConditionalWeakTable<DbContext, List<EntityEvents>> table, DbContext context)
    {
        if (!table.TryGetValue(context, out var events))
            return [];

        table.Remove(context);
        return events;
    }

    private async Task DispatchAsync(List<EntityEvents> collected, CancellationToken cancellationToken)
    {
        if (collected.Count == 0)
            return;

        try
        {
            var events = collected.SelectMany(c => c.Events).ToList();
            if (_dispatcher is not null)
            {
                await _dispatcher.DispatchAsync(events, cancellationToken);
            }
            else
            {
                await using var scope = _scopeFactory!.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>().DispatchAsync(events, cancellationToken);
            }
        }
        catch
        {
            Restore(collected);
            throw;
        }
    }

    private static void Restore(List<EntityEvents> collected)
    {
        foreach (var (entity, events) in collected)
        {
            foreach (var domainEvent in events)
                Invoke(entity, nameof(Entity<object>.AddDomainEvent), [typeof(IDomainEvent)], domainEvent);
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
