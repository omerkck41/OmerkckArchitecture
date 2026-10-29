using System.Diagnostics;
using AwesomeAssertions;
using Kck.Observability.OpenTelemetry.Tracing;
using Xunit;
using SpanKind = Kck.Observability.Abstractions.SpanKind;
using SpanStatus = Kck.Observability.Abstractions.SpanStatus;

namespace Kck.Observability.OpenTelemetry.Tests;

public sealed class OpenTelemetrySpanTests : IDisposable
{
    private readonly string _sourceName = "Kck.Tests." + Guid.NewGuid().ToString("N");
    private readonly ActivityListener _listener;
    private readonly List<Activity> _stopped = [];

    public OpenTelemetrySpanTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == _sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = a => _stopped.Add(a)
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose() => _listener.Dispose();

    [Theory]
    [InlineData(SpanKind.Server, ActivityKind.Server)]
    [InlineData(SpanKind.Client, ActivityKind.Client)]
    [InlineData(SpanKind.Producer, ActivityKind.Producer)]
    [InlineData(SpanKind.Consumer, ActivityKind.Consumer)]
    [InlineData(SpanKind.Internal, ActivityKind.Internal)]
    public void StartSpan_WithKind_CreatesActivityOfMatchingKind(SpanKind kind, ActivityKind expected)
    {
        var sut = new OpenTelemetryTracingService(_sourceName);

        using (sut.StartSpan("op", kind)) { }

        _stopped.Should().ContainSingle();
        _stopped[0].OperationName.Should().Be("op");
        _stopped[0].Kind.Should().Be(expected);
    }

    [Fact]
    public void SetAttribute_AddsTagToActivity()
    {
        var sut = new OpenTelemetryTracingService(_sourceName);

        using (var span = sut.StartSpan("op"))
            span.SetAttribute("user.id", 42);

        _stopped[0].GetTagItem("user.id").Should().Be(42);
    }

    [Theory]
    [InlineData(SpanStatus.Ok, ActivityStatusCode.Ok)]
    [InlineData(SpanStatus.Error, ActivityStatusCode.Error)]
    [InlineData(SpanStatus.Unset, ActivityStatusCode.Unset)]
    public void SetStatus_MapsToActivityStatusCode(SpanStatus status, ActivityStatusCode expected)
    {
        var sut = new OpenTelemetryTracingService(_sourceName);

        using (var span = sut.StartSpan("op"))
            span.SetStatus(status, "why");

        _stopped[0].Status.Should().Be(expected);
        if (expected == ActivityStatusCode.Error)
            _stopped[0].StatusDescription.Should().Be("why");
    }

    [Fact]
    public void RecordException_AddsExceptionEventAndMarksError()
    {
        var sut = new OpenTelemetryTracingService(_sourceName);
        var ex = new InvalidOperationException("bad thing");

        using (var span = sut.StartSpan("op"))
            span.RecordException(ex);

        var activity = _stopped[0];
        activity.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("bad thing");
        var ev = activity.Events.Should().ContainSingle().Subject;
        ev.Name.Should().Be("exception");
        ev.Tags.Should().Contain(t => t.Key == "exception.type" && (string?)t.Value == typeof(InvalidOperationException).FullName);
        ev.Tags.Should().Contain(t => t.Key == "exception.message" && (string?)t.Value == "bad thing");
    }

    [Fact]
    public void StartChild_CreatesNestedActivityWithParentAndKind()
    {
        var sut = new OpenTelemetryTracingService(_sourceName);

        using (var parent = sut.StartSpan("parent"))
        {
            using (parent.StartChild("child", SpanKind.Client)) { }
        }

        var child = _stopped.Single(a => a.OperationName == "child");
        var parentActivity = _stopped.Single(a => a.OperationName == "parent");
        child.Kind.Should().Be(ActivityKind.Client);
        child.ParentId.Should().Be(parentActivity.Id);
    }

    [Fact]
    public void Operations_OnSpanWithoutActivity_DoNotThrowAndStartChildYieldsInertSpan()
    {
        // No listener for this source => StartActivity returns null.
        var sut = new OpenTelemetryTracingService("Kck.Tests.Unlistened." + Guid.NewGuid().ToString("N"));
        var span = sut.StartSpan("op");

        span.SetAttribute("k", "v");
        span.SetStatus(SpanStatus.Ok);
        span.RecordException(new InvalidOperationException("x"));
        var child = span.StartChild("child");
        child.SetAttribute("k", "v");
        child.Dispose();
        span.Dispose();

        child.Should().NotBeNull();
        _stopped.Should().BeEmpty();
    }

    [Fact]
    public void MapKind_UnknownValue_FallsBackToInternal()
    {
        OpenTelemetrySpan.MapKind((SpanKind)999).Should().Be(ActivityKind.Internal);
    }
}
