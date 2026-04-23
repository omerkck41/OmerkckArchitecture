namespace Kck.BackgroundJobs.Hangfire;

public sealed class HangfireOptions
{
    /// <summary>Storage type: "InMemory" (only built-in backend). Add custom backends via Hangfire's own API.</summary>
    public string StorageType { get; set; } = "InMemory";

    /// <summary>Connection string for the storage backend.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Number of worker threads.</summary>
    public int WorkerCount { get; set; } = Environment.ProcessorCount;
}
