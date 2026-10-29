using AwesomeAssertions;
using Kck.Observability.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;
using KckHealthCheckResult = Kck.Observability.Abstractions.HealthCheckResult;
using KckHealthStatus = Kck.Observability.Abstractions.HealthStatus;

namespace Kck.Observability.OpenTelemetry.Tests;

public sealed class KckObservabilityRegistrationTests
{
    private static readonly string[] TagNames = ["live", "ready", "startup"];
    private static readonly string[] AllNames = ["plain", "live", "ready", "startup"];

    [Fact]
    public void AddKckObservability_ReturnsSameCollectionAndInvokesConfigure()
    {
        var services = new ServiceCollection();
        var called = false;

        var result = services.AddKckObservability(_ => called = true);

        result.Should().BeSameAs(services);
        called.Should().BeTrue();
    }

    [Fact]
    public void UseOpenTelemetry_RegistersTracingAndMetricsServices()
    {
        var services = new ServiceCollection();

        services.AddKckObservability(o => o.UseOpenTelemetry(t => t.ServiceName = "Svc"));

        services.Should().Contain(d => d.ServiceType == typeof(ITracingService) && d.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(d => d.ServiceType == typeof(IMetricsService) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseOpenTelemetry_ExistingTracingRegistration_IsNotReplaced()
    {
        var services = new ServiceCollection();
        var custom = new FakeTracing();
        services.AddSingleton<ITracingService>(custom);

        services.AddKckObservability(o => o.UseOpenTelemetry(_ => { }));

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ITracingService>().Should().BeSameAs(custom);
    }

    [Fact]
    public void UseOpenTelemetry_AllFeaturesEnabled_BuildsProviderAndResolvesServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckObservability(o => o.UseOpenTelemetry(t =>
        {
            t.ServiceName = "Kck.Tests.Full";
            t.EnableTracing().EnableMetrics().UseOtlpExporter("http://localhost:4317");
        }));

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ITracingService>().Should().NotBeNull();
        provider.GetRequiredService<IMetricsService>().Should().NotBeNull();
    }

    [Fact]
    public void KckOpenTelemetryBuilder_FluentMethods_ReturnSameBuilder()
    {
        var builder = new KckOpenTelemetryBuilder(new ServiceCollection());

        builder.EnableTracing().Should().BeSameAs(builder);
        builder.EnableMetrics().Should().BeSameAs(builder);
        builder.UseOtlpExporter("http://x").Should().BeSameAs(builder);
        builder.ServiceName.Should().Be("KckApp");
    }

    [Fact]
    public void UseHealthChecks_RegistersChecksWithExpectedTags()
    {
        var services = new ServiceCollection();
        var healthy = Task.FromResult(new KckHealthCheckResult(KckHealthStatus.Healthy));

        services.AddKckObservability(o => o.UseHealthChecks(h => h
            .AddCheck("plain", _ => healthy)
            .AddCheck("obj", new StubCheck())
            .AddLivenessCheck("live", _ => healthy)
            .AddReadinessCheck("ready", _ => healthy)
            .AddStartupCheck("startup", _ => healthy)));

        using var provider = services.BuildServiceProvider();
        var regs = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations
            .ToDictionary(r => r.Name, r => r.Tags);
        regs.Keys.Should().BeEquivalentTo("plain", "obj", "live", "ready", "startup");
        regs["plain"].Should().BeEmpty();
        regs["obj"].Should().BeEmpty();
        regs["live"].Should().BeEquivalentTo(["live"]);
        regs["ready"].Should().BeEquivalentTo(["ready"]);
        regs["startup"].Should().BeEquivalentTo(["startup"]);
    }

    [Fact]
    public async Task UseHealthChecks_ObjectCheck_RunsThroughHealthCheckService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckObservability(o => o.UseHealthChecks(h => h.AddCheck("obj", new StubCheck())));
        await using var provider = services.BuildServiceProvider();

        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        report.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);
        report.Entries["obj"].Description.Should().Be("stub");
    }

    [Theory]
    [InlineData("/health/live", "live", "plain")]
    [InlineData("/health/ready", "ready", "plain")]
    [InlineData("/health/startup", "startup", "plain")]
    public async Task MapKckHealthChecks_Endpoint_ReturnsOnlyMatchingTaggedAndUntaggedChecks(
        string path, string includedName, string untaggedName)
    {
        var json = await CallHealthEndpointAsync(path);

        json.Should().Contain($"\"name\":\"{includedName}\"");
        json.Should().Contain($"\"name\":\"{untaggedName}\"");
        foreach (var other in TagNames.Where(n => n != includedName))
            json.Should().NotContain($"\"name\":\"{other}\"");
    }

    [Fact]
    public async Task MapKckHealthChecks_ResponseShape_IsStatusChecksAndTotalDuration()
    {
        var json = await CallHealthEndpointAsync("/health/live");

        using var document = System.Text.Json.JsonDocument.Parse(json);
        var root = document.RootElement;
        root.EnumerateObject().Select(p => p.Name).Should().Equal("status", "checks", "totalDuration");
        root.GetProperty("totalDuration").ValueKind.Should().Be(System.Text.Json.JsonValueKind.Number);
        var check = root.GetProperty("checks").EnumerateArray().First();
        check.EnumerateObject().Select(p => p.Name).Should().Equal("name", "status", "description", "duration");
        check.GetProperty("duration").ValueKind.Should().Be(System.Text.Json.JsonValueKind.Number);
    }

    [Fact]
    public async Task MapKckHealthChecks_AggregateEndpoint_ReturnsAllChecksAsCamelCaseJson()
    {
        var json = await CallHealthEndpointAsync("/health");

        json.Should().Contain("\"status\":\"Healthy\"");
        json.Should().Contain("\"totalDuration\":");
        foreach (var n in AllNames)
            json.Should().Contain($"\"name\":\"{n}\"");
    }

    [Fact]
    public async Task MapKckHealthChecks_CustomBasePath_ServesUnderThatPath()
    {
        var json = await CallHealthEndpointAsync("/ops/live", "/ops");

        json.Should().Contain("\"name\":\"live\"");
    }

    private static async Task<string> CallHealthEndpointAsync(string path, string basePath = "/health")
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var healthy = Task.FromResult(new KckHealthCheckResult(KckHealthStatus.Healthy, "fine"));
        services.AddKckObservability(o => o.UseHealthChecks(h => h
            .AddCheck("plain", _ => healthy)
            .AddLivenessCheck("live", _ => healthy)
            .AddReadinessCheck("ready", _ => healthy)
            .AddStartupCheck("startup", _ => healthy)));
        await using var provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);
        app.MapKckHealthChecks(basePath);
        var pipeline = app.Build();

        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        await pipeline(context);

        context.Response.ContentType.Should().Be("application/json");
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private sealed class StubCheck : Kck.Observability.Abstractions.IHealthCheck
    {
        public Task<KckHealthCheckResult> CheckAsync(CancellationToken ct = default) =>
            Task.FromResult(new KckHealthCheckResult(KckHealthStatus.Degraded, "stub"));
    }

    private sealed class FakeTracing : ITracingService
    {
        public ISpan StartSpan(string name, SpanKind kind = SpanKind.Internal) => throw new NotSupportedException();
    }
}
