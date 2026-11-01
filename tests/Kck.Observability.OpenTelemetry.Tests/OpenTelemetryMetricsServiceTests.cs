using System.Diagnostics.Metrics;
using AwesomeAssertions;
using Kck.Observability.Abstractions;
using Kck.Observability.OpenTelemetry.Metrics;
using Xunit;

namespace Kck.Observability.OpenTelemetry.Tests;

public sealed class OpenTelemetryMetricsServiceTests
{
    private readonly string _meterName = "Kck.Tests." + Guid.NewGuid().ToString("N");

    [Fact]
    public void CreateCounter_ReturnsNonNullCounter()
    {
        var sut = new OpenTelemetryMetricsService("TestApp");

        var counter = sut.CreateCounter("test_counter", "A test counter");

        counter.Should().NotBeNull();
    }

    [Fact]
    public void CreateHistogram_ReturnsNonNullHistogram()
    {
        var sut = new OpenTelemetryMetricsService("TestApp");

        var histogram = sut.CreateHistogram("test_histogram", "A test histogram");

        histogram.Should().NotBeNull();
    }

    [Fact]
    public void CreateGauge_ReturnsNonNullGauge()
    {
        var sut = new OpenTelemetryMetricsService("TestApp");

        var gauge = sut.CreateGauge("test_gauge", "A test gauge");

        gauge.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceName_UsesDefaultMeterName()
    {
        var sut = new OpenTelemetryMetricsService();

        var counter = sut.CreateCounter("default_counter");

        counter.Should().NotBeNull();
    }

    [Fact]
    public void CreateHistogram_WithBuckets_PublishesThemAsBucketBoundaryAdvice()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);
        Instrument? published = null;
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (instrument.Meter.Name == _meterName && instrument.Name == "latency")
                    published = instrument;
            },
        };
        listener.Start();

        sut.CreateHistogram("latency", "Latency (ms)", [1, 5, 10]);

        var advice = published.Should().BeOfType<Histogram<double>>().Which.Advice;
        advice.Should().NotBeNull("the buckets are passed as instrument advice");
        advice!.HistogramBucketBoundaries.Should().Equal(1d, 5d, 10d);
    }

    [Fact]
    public void CreateHistogram_SameNameTwice_ReturnsTheSameInstance()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);

        var first = sut.CreateHistogram("x");
        var second = sut.CreateHistogram("x");

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void CreateCounter_SameNameTwice_ReturnsTheSameInstance()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);

        var first = sut.CreateCounter("c");
        var second = sut.CreateCounter("c");

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void CreateGauge_SameNameTwice_ReturnsTheSameInstance()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);

        var first = sut.CreateGauge("g");
        var second = sut.CreateGauge("g");

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void CachedInstruments_StillDeliverMeasurements()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);
        var seen = new List<(string Instrument, double Value)>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == _meterName)
                    l.EnableMeasurementEvents(instrument);
            },
        };
        listener.SetMeasurementEventCallback<double>((instrument, value, _, _) => seen.Add((instrument.Name, value)));
        listener.Start();

        sut.CreateCounter("hits").Increment(2);
        sut.CreateCounter("hits").Increment();
        sut.CreateHistogram("took", buckets: [1, 2]).Record(1.5);
        sut.CreateHistogram("took").Record(3);

        seen.Should().Equal(("hits", 2d), ("hits", 1d), ("took", 1.5d), ("took", 3d));
    }

    [Theory]
    [InlineData(new[] { 5d, 1d, 10d })]                 // unsorted
    [InlineData(new[] { 1d, 5d, 5d })]                  // duplicate
    [InlineData(new[] { 1d, double.NaN })]              // not finite
    [InlineData(new[] { 1d, double.PositiveInfinity })] // not finite
    public void CreateHistogram_InvalidBuckets_ThrowsArgumentExceptionNamingTheParameter(double[] buckets)
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);

        var act = () => sut.CreateHistogram("bad_buckets", buckets: buckets);

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("buckets");
    }

    [Fact]
    public void CreateHistogram_AfterInvalidBuckets_SameNameWithValidBucketsCreatesTheInstrument()
    {
        using var sut = new OpenTelemetryMetricsService(_meterName);
        var invalid = () => sut.CreateHistogram("retry", buckets: [5, 1]);
        invalid.Should().Throw<ArgumentException>();
        var seen = new List<double>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == _meterName && instrument.Name == "retry")
                    l.EnableMeasurementEvents(instrument);
            },
        };
        listener.SetMeasurementEventCallback<double>((_, value, _, _) => seen.Add(value));
        listener.Start();

        var histogram = sut.CreateHistogram("retry", buckets: [1, 5]);
        histogram.Record(2);

        seen.Should().Equal(2d);
    }

    [Fact]
    public void CreateCounter_HasNoTagKeysParameter()
    {
        // Tag keys without values have no OpenTelemetry representation: tags are passed on each Increment call.
        var parameters = typeof(IMetricsService).GetMethod(nameof(IMetricsService.CreateCounter))!.GetParameters();

        parameters.Select(p => p.Name).Should().Equal("name", "description");
    }
}
