using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;

namespace Kck.Logging.Serilog.Tests;

public class KckSerilogBuilderTests
{
    [Fact]
    public void WriteToConsole_ShouldToggleConsoleOutputOnAndJsonOff()
    {
        var builder = new KckSerilogBuilder { UseCompactJson = true, UseConsoleOutput = false };

        builder.WriteToConsole();

        builder.UseConsoleOutput.Should().BeTrue();
        builder.UseCompactJson.Should().BeFalse();
    }

    [Fact]
    public void WriteToCompactJson_ShouldToggleJsonOnAndConsoleOff()
    {
        var builder = new KckSerilogBuilder();

        builder.WriteToCompactJson();

        builder.UseCompactJson.Should().BeTrue();
        builder.UseConsoleOutput.Should().BeFalse();
    }

    [Fact]
    public void WriteToFile_ShouldSetPathAndInterval()
    {
        var builder = new KckSerilogBuilder();

        builder.WriteToFile("logs/app.log", RollingInterval.Hour);

        builder.FilePath.Should().Be("logs/app.log");
        builder.RollingInterval.Should().Be(RollingInterval.Hour);
    }

    [Fact]
    public void WriteToFile_DefaultInterval_ShouldBeDay()
    {
        var builder = new KckSerilogBuilder();

        builder.WriteToFile("logs/app.log");

        builder.RollingInterval.Should().Be(RollingInterval.Day);
    }

    [Fact]
    public void WithApplicationName_ShouldSetApplicationName()
    {
        var builder = new KckSerilogBuilder();

        builder.WithApplicationName("OrderService");

        builder.ApplicationName.Should().Be("OrderService");
    }

    [Fact]
    public void WithTraceCorrelation_ShouldToggleEnricher()
    {
        var builder = new KckSerilogBuilder();

        builder.WithTraceCorrelation(enabled: false);

        builder.EnrichWithTraceContext.Should().BeFalse();
    }

    [Fact]
    public void Configure_ShouldSetAdditionalConfig()
    {
        var builder = new KckSerilogBuilder();
        var called = false;
        Action<LoggerConfiguration> config = _ => called = true;

        builder.Configure(config);
        builder.AdditionalConfig!.Invoke(new LoggerConfiguration());

        called.Should().BeTrue();
    }

    [Fact]
    public void Defaults_ShouldMatchExpectedValues()
    {
        var builder = new KckSerilogBuilder();

        builder.UseConsoleOutput.Should().BeTrue();
        builder.UseCompactJson.Should().BeFalse();
        builder.EnrichWithTraceContext.Should().BeTrue();
        builder.RollingInterval.Should().Be(RollingInterval.Day);
        builder.RetainedFileCount.Should().Be(31);
        builder.ApplicationName.Should().BeNull();
        builder.FilePath.Should().BeNull();
    }

    [Fact]
    public void BuilderChain_ShouldReturnSameInstance()
    {
        var builder = new KckSerilogBuilder();

        var chained = builder
            .WithApplicationName("App")
            .WithTraceCorrelation()
            .WriteToConsole();

        chained.Should().BeSameAs(builder);
    }
}
