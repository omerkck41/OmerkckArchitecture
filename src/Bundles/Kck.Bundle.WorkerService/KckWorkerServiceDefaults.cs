namespace Kck.Bundle.WorkerService;

public sealed class KckWorkerServiceDefaults
{
    public BackgroundJobsDefaults BackgroundJobs { get; set; } = new();
    public ObservabilityDefaults Observability { get; set; } = new();
}

public sealed class BackgroundJobsDefaults
{
    /// <summary>Use "Hangfire" or "Quartz". Only one provider is supported at a time.</summary>
    public string Provider { get; set; } = "Hangfire";

    public int HangfireWorkerCount { get; set; } = 5;
}

public sealed class ObservabilityDefaults
{
    public bool Tracing { get; set; } = true;
    public bool Metrics { get; set; } = true;
    public string? OtlpEndpoint { get; set; }
    public string? ServiceName { get; set; }
}
