namespace Kck.Observability.Abstractions;

public interface ITracingService
{
    ISpan StartSpan(string name, SpanKind kind = SpanKind.Internal);
}
