using FluentAssertions;
using Kck.Observability.OpenTelemetry.Tracing;
using Xunit;

namespace Kck.Observability.OpenTelemetry.Tests;

public sealed class OpenTelemetryTracingServiceTests
{
    [Fact]
    public void StartSpan_ReturnsNonNullSpan()
    {
        var sut = new OpenTelemetryTracingService("TestApp");

        var span = sut.StartSpan("test-operation");

        span.Should().NotBeNull();
    }

    [Fact]
    public void StartSpan_WithInternalKind_ReturnsSpan()
    {
        var sut = new OpenTelemetryTracingService("TestApp");

        var span = sut.StartSpan("internal-op", Kck.Observability.Abstractions.SpanKind.Internal);

        span.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceName_UsesDefaultSource()
    {
        var sut = new OpenTelemetryTracingService();

        var span = sut.StartSpan("default-op");

        span.Should().NotBeNull();
    }
}
