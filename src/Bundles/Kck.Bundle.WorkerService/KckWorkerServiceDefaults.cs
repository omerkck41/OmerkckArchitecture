namespace Kck.Bundle.WorkerService;

public sealed class KckWorkerServiceDefaults
{
    public BackgroundJobsDefaults BackgroundJobs { get; set; } = new();
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
