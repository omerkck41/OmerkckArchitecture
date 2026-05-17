namespace Kck.Observability.Abstractions;

/// <summary>
/// Represents an active distributed trace span; supports attribute annotation, status setting, exception recording, and child span creation.
/// </summary>
public interface ISpan : IDisposable
{
    /// <summary>Attaches a key-value attribute to the span for contextual metadata.</summary>
    void SetAttribute(string key, object value);

    /// <summary>Sets the completion status of the span, with an optional description for error cases.</summary>
    void SetStatus(SpanStatus status, string? description = null);

    /// <summary>Records the given <paramref name="exception"/> as an event on the span.</summary>
    void RecordException(Exception exception);

    /// <summary>Creates and starts a child span nested under this span with the specified <paramref name="name"/> and <paramref name="kind"/>.</summary>
    ISpan StartChild(string name, SpanKind kind = SpanKind.Internal);
}

/// <summary>
/// Describes the role of a span within a distributed trace, following OpenTelemetry span kind semantics.
/// </summary>
public enum SpanKind
{
    /// <summary>An internal operation not crossing a service boundary.</summary>
    Internal,

    /// <summary>A span that handles an incoming synchronous remote call.</summary>
    Server,

    /// <summary>A span that issues an outgoing synchronous remote call.</summary>
    Client,

    /// <summary>A span that publishes a message to a messaging system.</summary>
    Producer,

    /// <summary>A span that receives and processes a message from a messaging system.</summary>
    Consumer
}

/// <summary>
/// Indicates the outcome status of a completed span.
/// </summary>
public enum SpanStatus
{
    /// <summary>No status has been explicitly set; the default state.</summary>
    Unset,

    /// <summary>The operation completed successfully.</summary>
    Ok,

    /// <summary>The operation ended with an error.</summary>
    Error
}
