using FluentAssertions;
using Kck.Observability.OpenTelemetry.Metrics;
using Xunit;

namespace Kck.Observability.OpenTelemetry.Tests;

public sealed class OpenTelemetryMetricsServiceTests
{
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
}
