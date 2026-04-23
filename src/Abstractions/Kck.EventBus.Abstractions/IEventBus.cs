namespace Kck.EventBus.Abstractions;

/// <summary>
/// Defines the contract for publishing and subscribing to integration events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : IntegrationEvent;

    /// <summary>
    /// Subscribes a handler to a specific integration event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>;
}
