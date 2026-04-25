namespace Kck.EventBus.Abstractions;

/// <summary>
/// Base record for integration events used in event-driven communication.
/// </summary>
public abstract record IntegrationEvent
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    public Guid Id { get; init; } =
#if NET9_0_OR_GREATER
        Guid.CreateVersion7();
#else
        Guid.NewGuid();
#endif

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
