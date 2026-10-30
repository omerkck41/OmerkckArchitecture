using System.Diagnostics.CodeAnalysis;

namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Handles one domain event type. Register handlers with <c>AddDomainEventHandlers</c> (Kck.Persistence.EntityFramework);
/// they run after the aggregate is persisted, and after the transaction commits when one is open.
/// </summary>
/// <remarks>
/// A handler receives events of exactly <typeparamref name="TEvent"/>, not of derived types. Delivery is at least once:
/// when a handler fails, the events go back onto the entity and the next save dispatches them again, so handlers must be
/// idempotent. The order of several handlers for the same event is not defined. Exact-type matching holds with
/// Microsoft.Extensions.DependencyInjection; a container that resolves contravariant registrations may also return
/// handlers of base event types.
/// </remarks>
/// <typeparam name="TEvent">The domain event type handled.</typeparam>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "IDomainEventHandler is the established domain-driven design term (ADR-0030).")]
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>Handles <paramref name="domainEvent"/>.</summary>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
