using System.Diagnostics;
using Kck.Observability.Abstractions;
using SpanKind = Kck.Observability.Abstractions.SpanKind;
using SpanStatus = Kck.Observability.Abstractions.SpanStatus;

namespace Kck.Observability.OpenTelemetry.Tracing;

/// <summary>
/// Wraps an <see cref="Activity"/> as an <see cref="ISpan"/>, forwarding attribute, status, exception, and child-span operations to the OpenTelemetry activity API.
/// </summary>
internal sealed class OpenTelemetrySpan(Activity? activity) : ISpan
{
    /// <summary>Sets a tag on the underlying activity with the given key and value.</summary>
    public void SetAttribute(string key, object value)
    {
        activity?.SetTag(key, value);
    }

    /// <summary>Maps a <see cref="SpanStatus"/> to an <see cref="ActivityStatusCode"/> and applies it to the underlying activity.</summary>
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

    /// <summary>Adds an exception event to the activity with standard OpenTelemetry attributes and marks the span as errored.</summary>
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

    /// <summary>Starts a new child span as a nested <see cref="Activity"/> and returns it wrapped as an <see cref="ISpan"/>.</summary>
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
