namespace Kck.Observability.Abstractions;

public interface ISpan : IDisposable
{
    void SetAttribute(string key, object value);
    void SetStatus(SpanStatus status, string? description = null);
    void RecordException(Exception exception);
    ISpan StartChild(string name, SpanKind kind = SpanKind.Internal);
}

public enum SpanKind
{
    Internal,
    Server,
    Client,
    Producer,
    Consumer
}

public enum SpanStatus
{
    Unset,
    Ok,
    Error
}
