#pragma warning disable CA1848, CA1873 // tests exercise the logging pipeline with plain templates
using System.Diagnostics;
using System.Globalization;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Kck.Logging.Serilog.Tests;

public sealed class SerilogPipelineTests
{
    private sealed class CapturingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    private static (ILogger<SerilogPipelineTests> Logger, CapturingSink Sink, ServiceProvider Provider) Build(
        Action<KckSerilogBuilder>? configure = null)
    {
        var sink = new CapturingSink();
        var services = new ServiceCollection();
        services.AddKckSerilog(b =>
        {
            b.UseConsoleOutput = false;
            b.Configure(c => c.WriteTo.Sink(sink));
            configure?.Invoke(b);
        });

        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<ILoggerFactory>().CreateLogger<SerilogPipelineTests>(), sink, provider);
    }

    private static string Scalar(LogEvent evt, string key) =>
        ((ScalarValue)evt.Properties[key]).Value!.ToString()!;

    private static ActivityListener CreateListener(ActivitySource source)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == source.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private static string ReadShared(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static void TryDelete(string dir)
    {
        try
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
        catch (IOException)
        {
            // Serilog keeps the file handle open until the process exits; the temp dir is disposable.
        }
    }

    [Fact]
    public void AddKckSerilog_NoApplicationName_UsesDefaultKckApp()
    {
        var (logger, sink, provider) = Build();
        using var _ = provider;

        logger.LogInformation("hello");

        Scalar(sink.Events.Single(), "Application").Should().Be("KckApp");
    }

    [Fact]
    public void AddKckSerilog_WithApplicationName_EnrichesEvents()
    {
        var (logger, sink, provider) = Build(b => b.WithApplicationName("OrderService"));
        using var _ = provider;

        logger.LogInformation("hello");

        Scalar(sink.Events.Single(), "Application").Should().Be("OrderService");
    }

    [Fact]
    public void AddKckSerilog_DebugMessage_IsFilteredByInformationMinimum()
    {
        var (logger, sink, provider) = Build();
        using var _ = provider;

        logger.LogDebug("hidden");
        logger.LogInformation("shown");

        sink.Events.Should().ContainSingle().Which.Level.Should().Be(LogEventLevel.Information);
    }

    [Fact]
    public void AddKckSerilog_UsePiiMasking_MasksDefaultProperties()
    {
        var (logger, sink, provider) = Build(b => b.UsePiiMasking());
        using var _ = provider;

        logger.LogInformation("login {Email} {Other}", "a@b.com", "visible");

        var evt = sink.Events.Single();
        Scalar(evt, "Email").Should().Be("***");
        Scalar(evt, "Other").Should().Be("visible");
    }

    [Fact]
    public void AddKckSerilog_MaskPiiProperties_MasksOnlyCustomNames()
    {
        var (logger, sink, provider) = Build(b => b.MaskPiiProperties("Nickname"));
        using var _ = provider;

        logger.LogInformation("x {Nickname} {Email}", "bob", "a@b.com");

        var evt = sink.Events.Single();
        Scalar(evt, "Nickname").Should().Be("***");
        Scalar(evt, "Email").Should().Be("a@b.com");
    }

    [Fact]
    public void AddKckSerilog_WithoutPiiConfiguration_DoesNotMask()
    {
        var (logger, sink, provider) = Build();
        using var _ = provider;

        logger.LogInformation("x {Password}", "secret");

        Scalar(sink.Events.Single(), "Password").Should().Be("secret");
    }

    [Fact]
    public void UsePiiMasking_DefaultList_MasksEveryDefaultName()
    {
        var (logger, sink, provider) = Build(b => b.UsePiiMasking());
        using var _ = provider;

        logger.LogInformation("{Password} {Token} {Ssn}", "p", "t", "s");

        var evt = sink.Events.Single();
        evt.Properties.Where(p => p.Key is "Password" or "Token" or "Ssn")
            .Should().HaveCount(3)
            .And.OnlyContain(p => ((ScalarValue)p.Value).Value!.ToString() == "***");
    }

    [Fact]
    public void AddKckSerilog_ActivityWithParent_AddsTraceSpanAndParentSpanIds()
    {
        var (logger, sink, provider) = Build();
        using var _ = provider;
        using var source = new ActivitySource("Kck.Logging.Serilog.Tests.Trace");
        using var listener = CreateListener(source);

        using var parent = source.StartActivity("parent")!;
        using var child = source.StartActivity("child")!;
        logger.LogInformation("traced");

        var evt = sink.Events.Single();
        Scalar(evt, "TraceId").Should().Be(child.TraceId.ToString());
        Scalar(evt, "SpanId").Should().Be(child.SpanId.ToString());
        Scalar(evt, "ParentSpanId").Should().Be(parent.SpanId.ToString());
    }

    [Fact]
    public void AddKckSerilog_RootActivity_OmitsParentSpanId()
    {
        var (logger, sink, provider) = Build();
        using var _ = provider;
        using var source = new ActivitySource("Kck.Logging.Serilog.Tests.Root");
        using var listener = CreateListener(source);

        using var root = source.StartActivity("root", ActivityKind.Internal, parentContext: default)!;
        logger.LogInformation("traced");

        var evt = sink.Events.Single();
        evt.Properties.Should().ContainKey("TraceId");
        evt.Properties.Should().NotContainKey("ParentSpanId");
    }

    [Fact]
    public void AddKckSerilog_NoActivity_OmitsTraceProperties()
    {
        var (logger, sink, provider) = Build();
        using var _ = provider;
        Activity.Current = null;

        logger.LogInformation("plain");

        sink.Events.Single().Properties.Should().NotContainKey("TraceId");
    }

    [Fact]
    public void AddKckSerilog_TraceCorrelationDisabled_OmitsTraceProperties()
    {
        var (logger, sink, provider) = Build(b => b.WithTraceCorrelation(enabled: false));
        using var _ = provider;
        using var activity = new Activity("op").Start();

        logger.LogInformation("plain");

        sink.Events.Single().Properties.Should().NotContainKey("TraceId");
    }

    [Fact]
    public void AddKckSerilog_WriteToFile_CreatesLogFileWithMessage()
    {
        var dir = Path.Join(Path.GetTempPath(), "kck-serilog-" + Guid.NewGuid().ToString("N"));
        try
        {
            var path = Path.Join(dir, "app-.log");
            var services = new ServiceCollection();
            services.AddKckSerilog(b => b.WriteToFile(path, RollingInterval.Day).WriteToConsole());
            using (var provider = services.BuildServiceProvider())
            {
                // Disposing the provider flushes the file sink before the file is read.
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<SerilogPipelineTests>().LogInformation("to file {Value}", 42);
            }

            Directory.Exists(dir).Should().BeTrue();
            var file = Directory.GetFiles(dir).Should().ContainSingle().Subject;
            ReadShared(file).Should().Contain("to file 42");
        }
        finally
        {
            TryDelete(dir);
        }
    }

    [Fact]
    public void AddKckSerilog_CompactJson_BuildsAndLogsWithoutThrowing()
    {
        var services = new ServiceCollection();
        services.AddKckSerilog(b => b.WriteToCompactJson());
        using var json = services.BuildServiceProvider();

        var act = () => json.GetRequiredService<ILoggerFactory>().CreateLogger<SerilogPipelineTests>().LogWarning("json");

        act.Should().NotThrow();
    }

    [Fact]
    public void AddKckSerilog_MaskedProperty_RenderedMessageHidesValue()
    {
        var (logger, sink, provider) = Build(b => b.MaskPiiProperties("Card"));
        using var _ = provider;

        logger.LogInformation("pay {Card}", "4111");

        sink.Events.Single().RenderMessage(CultureInfo.InvariantCulture).Should().Contain("***").And.NotContain("4111");
    }
}
