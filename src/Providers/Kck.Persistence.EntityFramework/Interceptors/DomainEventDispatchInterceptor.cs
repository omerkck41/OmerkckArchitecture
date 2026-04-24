using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// EF Core interceptor that collects and dispatches domain events after SaveChanges.
/// </summary>
public sealed partial class DomainEventDispatchInterceptor(
    IDomainEventDispatcher dispatcher,
    ILogger<DomainEventDispatchInterceptor> logger) : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);

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
            Task.Run(() => DispatchDomainEventsAsync(eventData.Context, CancellationToken.None)).GetAwaiter().GetResult();
        }

        return result;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sync SavedChanges triggered domain event dispatch. Prefer SaveChangesAsync to avoid thread pool offloading")]
    private static partial void LogSyncSaveChanges(ILogger logger);

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var entitiesWithEvents = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IEntity<object> || HasDomainEvents(e.Entity))
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = new List<IDomainEvent>();

        foreach (var entity in entitiesWithEvents)
        {
            if (entity is not Entity<object> domainEntity)
            {
                // Try to collect events via reflection for entities with non-object TId
                var eventsProperty = entity.GetType().GetProperty(nameof(Entity<object>.DomainEvents));
                if (eventsProperty?.GetValue(entity) is IReadOnlyCollection<IDomainEvent> events && events.Count > 0)
                {
                    domainEvents.AddRange(events);
                    entity.GetType().GetMethod(nameof(Entity<object>.ClearDomainEvents))?.Invoke(entity, null);
                }

                continue;
            }

            if (domainEntity.DomainEvents.Count == 0)
                continue;

            domainEvents.AddRange(domainEntity.DomainEvents);
            domainEntity.ClearDomainEvents();
        }

        if (domainEvents.Count > 0)
            await dispatcher.DispatchAsync(domainEvents, cancellationToken);
    }

    private static bool HasDomainEvents(object entity)
    {
        var property = entity.GetType().GetProperty(nameof(Entity<object>.DomainEvents));
        return property?.GetValue(entity) is IReadOnlyCollection<IDomainEvent> { Count: > 0 };
    }
}
