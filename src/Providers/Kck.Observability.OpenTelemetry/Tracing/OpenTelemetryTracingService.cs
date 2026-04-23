using System.Diagnostics;
using Kck.Observability.Abstractions;
using SpanKind = Kck.Observability.Abstractions.SpanKind;

namespace Kck.Observability.OpenTelemetry.Tracing;

internal sealed class OpenTelemetryTracingService : ITracingService
{
    internal static readonly ActivitySource DefaultSource = new("Kck.Application");

    private readonly ActivitySource _activitySource;

    public OpenTelemetryTracingService(string? serviceName = null)
    {
        _activitySource = serviceName is not null
            ? new ActivitySource(serviceName)
            : DefaultSource;
    }

    public ISpan StartSpan(string name, SpanKind kind = SpanKind.Internal)
    {
        var activity = _activitySource.StartActivity(name, OpenTelemetrySpan.MapKind(kind));
        return new OpenTelemetrySpan(activity);
    }
}
