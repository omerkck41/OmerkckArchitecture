namespace Kck.Bundle.WorkerService;

public sealed class KckWorkerServiceDefaults
{
    public BackgroundJobsDefaults BackgroundJobs { get; set; } = new();
    [Obsolete(
        "Not read by AddKckWorkerServiceDefaults: " +
        "configure OpenTelemetry in code with AddKckObservability(o => o.UseOpenTelemetry(...)). Removed in v5.0.",
        DiagnosticId = "KCK0002",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    public ObservabilityDefaults Observability { get; set; } = new();
}

public sealed class BackgroundJobsDefaults
{
    /// <summary>
    /// Background job provider. Only "Quartz" is bundled; to use Hangfire, reference Kck.BackgroundJobs.Hangfire and
    /// call AddKckBackgroundJobsHangfire yourself instead of using this bundle's scheduler.
    /// </summary>
    public string Provider { get; set; } = "Quartz";
}

public sealed class ObservabilityDefaults
{
    public bool Tracing { get; set; } = true;
    public bool Metrics { get; set; } = true;
    public string? OtlpEndpoint { get; set; }
    public string? ServiceName { get; set; }
}
