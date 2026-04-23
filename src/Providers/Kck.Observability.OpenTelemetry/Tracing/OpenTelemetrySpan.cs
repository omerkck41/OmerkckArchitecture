using System.Diagnostics;
using Kck.Observability.Abstractions;
using SpanKind = Kck.Observability.Abstractions.SpanKind;
using SpanStatus = Kck.Observability.Abstractions.SpanStatus;

namespace Kck.Observability.OpenTelemetry.Tracing;

internal sealed class OpenTelemetrySpan(Activity? activity) : ISpan
{
    public void SetAttribute(string key, object value)
    {
        activity?.SetTag(key, value);
    }

    public void SetStatus(SpanStatus status, string? description = null)
    {
        if (activity is null) return;

        var activityStatus = status switch
        {
            SpanStatus.Ok => ActivityStatusCode.Ok,
            SpanStatus.Error => ActivityStatusCode.Error,
            _ => ActivityStatusCode.Unset
        };

        activity.SetStatus(activityStatus, description);
    }

    public void RecordException(Exception exception)
    {
        activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
        {
            { "exception.type", exception.GetType().FullName },
            { "exception.message", exception.Message },
            { "exception.stacktrace", exception.StackTrace }
        }));
        SetStatus(SpanStatus.Error, exception.Message);
    }

    public ISpan StartChild(string name, SpanKind kind = SpanKind.Internal)
    {
        var source = activity?.Source ?? OpenTelemetryTracingService.DefaultSource;
        var child = source.StartActivity(name, MapKind(kind));
        return new OpenTelemetrySpan(child);
    }

    public void Dispose()
    {
        activity?.Dispose();
    }

    internal static ActivityKind MapKind(SpanKind kind) => kind switch
    {
        SpanKind.Server => ActivityKind.Server,
        SpanKind.Client => ActivityKind.Client,
        SpanKind.Producer => ActivityKind.Producer,
        SpanKind.Consumer => ActivityKind.Consumer,
        _ => ActivityKind.Internal
    };
}
