namespace Kck.EventBus.Abstractions;

/// <summary>
/// Base record for integration events used in event-driven communication.
/// </summary>
public abstract record IntegrationEvent
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
