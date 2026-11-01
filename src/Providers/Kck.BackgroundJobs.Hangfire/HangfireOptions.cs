using Hangfire;

namespace Kck.BackgroundJobs.Hangfire;

public sealed class HangfireOptions
{
    /// <summary>Storage type: "InMemory" (only built-in backend). For another backend, set <see cref="ConfigureStorage"/>.</summary>
    public string StorageType { get; set; } = "InMemory";

    /// <summary>
    /// Configures the Hangfire storage instead of the in-memory one (for example <c>c.UseSqlServerStorage(...)</c> from
    /// the Hangfire.SqlServer package). When set, <see cref="StorageType"/> is ignored, and the delegate must select a
    /// storage: otherwise Hangfire fails when the server starts ("JobStorage.Current ... not initialized").
    /// </summary>
    public Action<IGlobalConfiguration>? ConfigureStorage { get; set; }

    /// <summary>Number of worker threads.</summary>
    public int WorkerCount { get; set; } = Environment.ProcessorCount;
}
