namespace Kck.Observability.Abstractions;

/// <summary>
/// Entry point for distributed tracing; creates root spans that correlate work across service boundaries.
/// </summary>
public interface ITracingService
{
    /// <summary>Starts and returns a new <see cref="ISpan"/> with the given <paramref name="name"/> and <paramref name="kind"/>.</summary>
    ISpan StartSpan(string name, SpanKind kind = SpanKind.Internal);
}
