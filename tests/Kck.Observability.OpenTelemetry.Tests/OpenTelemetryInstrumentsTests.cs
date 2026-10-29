using System.Diagnostics.Metrics;
using AwesomeAssertions;
using Kck.Observability.OpenTelemetry.Metrics;
using Xunit;

namespace Kck.Observability.OpenTelemetry.Tests;

public sealed class OpenTelemetryInstrumentsTests
{
    private readonly string _meterName = "Kck.Tests." + Guid.NewGuid().ToString("N");

    [Fact]
    public void Counter_Increment_EmitsMeasurementWithValueAndTags()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);
        var counter = sut.CreateCounter("requests", "desc");
        var seen = new List<(double Value, object? Tag)>();
        using var listener = CreateListener(seen, "requests");

        counter.Increment(3, new KeyValuePair<string, object?>("route", "/a"));
        counter.Increment();

        seen.Should().HaveCount(2);
        seen[0].Value.Should().Be(3);
        seen[0].Tag.Should().Be("/a");
        seen[1].Value.Should().Be(1);
        seen[1].Tag.Should().BeNull();
    }

    [Fact]
    public void Histogram_Record_EmitsMeasurement()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);
        var histogram = sut.CreateHistogram("latency");
        var seen = new List<(double Value, object? Tag)>();
        using var listener = CreateListener(seen, "latency");

        histogram.Record(12.5, new KeyValuePair<string, object?>("route", "/b"));

        seen.Should().ContainSingle().Which.Should().Be((12.5, (object?)"/b"));
    }

    [Fact]
    public void Gauge_Set_ReportsLastValueAndTagsOnObservation()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);
        var gauge = sut.CreateGauge("queue_depth", "depth");
        var seen = new List<(double Value, object? Tag)>();
        using var listener = CreateListener(seen, "queue_depth");

        gauge.Set(1, new KeyValuePair<string, object?>("q", "old"));
        gauge.Set(9, new KeyValuePair<string, object?>("q", "new"));
        listener.RecordObservableInstruments();

        seen.Should().ContainSingle().Which.Should().Be((9d, (object?)"new"));
    }

    [Fact]
    public void Gauge_BeforeAnySet_ReportsZero()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);
        sut.CreateGauge("fresh_gauge");
        var seen = new List<(double Value, object? Tag)>();
        using var listener = CreateListener(seen, "fresh_gauge");

        listener.RecordObservableInstruments();

        seen.Should().ContainSingle().Which.Value.Should().Be(0);
    }

    [Fact]
    public void Dispose_DisposesMeter_StopsEmission()
    {
        var sut = new OpenTelemetryMetricsService(_meterName);
        var counter = sut.CreateCounter("after_dispose");
        var seen = new List<(double Value, object? Tag)>();
        using var listener = CreateListener(seen, "after_dispose");

        sut.Dispose();
        counter.Increment(5);

        seen.Should().BeEmpty();
    }

    private MeterListener CreateListener(List<(double Value, object? Tag)> seen, string instrumentName)
    {
        var listener = new MeterListener
        {
            InstrumentPublished = (inst, l) =>
            {
                if (inst.Meter.Name == _meterName && inst.Name == instrumentName)
                    l.EnableMeasurementEvents(inst);
            }
        };
        listener.SetMeasurementEventCallback<double>((_, value, tags, _) =>
            seen.Add((value, tags.Length > 0 ? tags[0].Value : null)));
        listener.Start();
        return listener;
    }
}
