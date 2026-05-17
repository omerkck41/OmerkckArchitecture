using System.Diagnostics;
using Kck.Observability.Abstractions;
using SpanKind = Kck.Observability.Abstractions.SpanKind;

namespace Kck.Observability.OpenTelemetry.Tracing;

/// <summary>
/// OpenTelemetry implementation of <see cref="ITracingService"/> that starts spans as <see cref="Activity"/> instances via an <see cref="ActivitySource"/>.
/// </summary>
internal sealed class OpenTelemetryTracingService : ITracingService
{
    /// <summary>Shared default <see cref="ActivitySource"/> named <c>Kck.Application</c>, used when no service name is specified.</summary>
    internal static readonly ActivitySource DefaultSource = new("Kck.Application");

    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Initializes the service with a named <see cref="ActivitySource"/>, falling back to <see cref="DefaultSource"/> when <paramref name="serviceName"/> is <see langword="null"/>.
    /// </summary>
    public OpenTelemetryTracingService(string? serviceName = null)
    {
        _activitySource = serviceName is not null
            ? new ActivitySource(serviceName)
            : DefaultSource;
    }

    /// <summary>Starts a new root span with the given name and kind and returns it as an <see cref="ISpan"/>.</summary>
    public ISpan StartSpan(string name, SpanKind kind = SpanKind.Internal)
    {
        var activity = _activitySource.StartActivity(name, OpenTelemetrySpan.MapKind(kind));
        return new OpenTelemetrySpan(activity);
    }
}
