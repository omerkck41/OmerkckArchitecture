using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Kck.Logging.Serilog.Tests;

public class SerilogDiRegistrationTests
{
    [Fact]
    public void AddKckSerilog_ShouldRegisterLoggerFactory()
    {
        var services = new ServiceCollection();

        services.AddKckSerilog(b => b.WithApplicationName("UnitTest"));

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();

        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddKckSerilog_WithoutConfigure_ShouldRegisterLoggerFactory()
    {
        var services = new ServiceCollection();

        services.AddKckSerilog();

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();

        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddKckSerilog_ShouldAllowLoggingWithoutError()
    {
        var services = new ServiceCollection();
        services.AddKckSerilog(b => b.WithApplicationName("TestApp").WriteToConsole());

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        var logger = factory.CreateLogger<SerilogDiRegistrationTests>();

        var act = () => logger.LogInformation("test message {Value}", 42);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddKckSerilog_WithTraceContext_ShouldCaptureActivity()
    {
        using var activity = new Activity("TestOp").Start();

        var services = new ServiceCollection();
        services.AddKckSerilog(b => b.WithTraceCorrelation());

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        var logger = factory.CreateLogger("trace-test");

        var act = () => logger.LogInformation("with trace");
        act.Should().NotThrow();
        activity.TraceId.Should().NotBe(default);
    }
}
