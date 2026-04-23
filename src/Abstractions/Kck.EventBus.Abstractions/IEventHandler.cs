using System.Diagnostics.CodeAnalysis;

namespace Kck.EventBus.Abstractions;

/// <summary>
/// Defines a handler for a specific integration event type.
/// </summary>
/// <typeparam name="TEvent">The type of integration event to handle.</typeparam>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "IEventHandler is a domain convention in event-driven architecture.")]
public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    /// <summary>
    /// Handles the specified integration event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
