namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Marker contract for domain events. Implement on record/class types that represent a significant
/// occurrence within the domain. Dispatch via <c>IDomainEventDispatcher</c> after the aggregate
/// is persisted to guarantee delivery within the same transaction.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier for this event instance.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp when the domain event occurred.</summary>
    DateTime OccurredOn { get; }
}
