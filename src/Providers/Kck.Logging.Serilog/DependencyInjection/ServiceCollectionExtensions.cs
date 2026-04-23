using System.Diagnostics;
using Serilog;
using Serilog.Formatting.Compact;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckLoggingSerilogServiceCollectionExtensions
{
    public static IServiceCollection AddKckSerilog(
        this IServiceCollection services,
        Action<KckSerilogBuilder>? configure = null)
    {
        var builder = new KckSerilogBuilder();
        configure?.Invoke(builder);

        services.AddSerilog(loggerConfig =>
        {
            loggerConfig
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", builder.ApplicationName ?? "KckApp");

            if (builder.EnrichWithTraceContext)
            {
                loggerConfig.Enrich.With(new TraceContextEnricher());
            }

            if (builder.UseConsoleOutput)
            {
                loggerConfig.WriteTo.Console();
            }

            if (builder.UseCompactJson)
            {
                loggerConfig.WriteTo.Console(new RenderedCompactJsonFormatter());
            }

            if (builder.FilePath is not null)
            {
                loggerConfig.WriteTo.File(
                    builder.FilePath,
                    rollingInterval: builder.RollingInterval,
                    retainedFileCountLimit: builder.RetainedFileCount);
            }

            builder.AdditionalConfig?.Invoke(loggerConfig);
        });

        return services;
    }
}

public sealed class KckSerilogBuilder
{
    public bool UseConsoleOutput { get; set; } = true;
    public bool UseCompactJson { get; set; }
    public bool EnrichWithTraceContext { get; set; } = true;
    public string? ApplicationName { get; set; }
    public string? FilePath { get; set; }
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
    public int RetainedFileCount { get; set; } = 31;
    public Action<LoggerConfiguration>? AdditionalConfig { get; set; }

    public KckSerilogBuilder WriteToConsole()
    {
        UseConsoleOutput = true;
        UseCompactJson = false;
        return this;
    }

    public KckSerilogBuilder WriteToCompactJson()
    {
        UseCompactJson = true;
        UseConsoleOutput = false;
        return this;
    }

    public KckSerilogBuilder WriteToFile(string path, RollingInterval interval = RollingInterval.Day)
    {
        FilePath = path;
        RollingInterval = interval;
        return this;
    }

    public KckSerilogBuilder WithApplicationName(string name)
    {
        ApplicationName = name;
        return this;
    }

    public KckSerilogBuilder WithTraceCorrelation(bool enabled = true)
    {
        EnrichWithTraceContext = enabled;
        return this;
    }

    public KckSerilogBuilder Configure(Action<LoggerConfiguration> config)
    {
        AdditionalConfig = config;
        return this;
    }
}

internal sealed class TraceContextEnricher : Serilog.Core.ILogEventEnricher
{
    public void Enrich(Serilog.Events.LogEvent logEvent, Serilog.Core.ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity is null) return;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));

        if (activity.ParentSpanId != default)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ParentSpanId", activity.ParentSpanId.ToString()));
        }
    }
}
