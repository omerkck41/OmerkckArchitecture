using Hangfire;

namespace Kck.BackgroundJobs.Hangfire;

public sealed class HangfireOptions
{
    /// <summary>Storage type: "InMemory" (only built-in backend). For another backend, set <see cref="ConfigureStorage"/>.</summary>
    public string StorageType { get; set; } = "InMemory";

    /// <summary>Never read: configure a storage that needs a connection string through <see cref="ConfigureStorage"/>.</summary>
    [Obsolete(
        "Never read by AddKckBackgroundJobsHangfire. Configure the storage with ConfigureStorage, for example " +
        "o.ConfigureStorage = c => c.UseSqlServerStorage(connectionString). Removed in v5.0.",
        DiagnosticId = "KCK0700",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Configures the Hangfire storage instead of the in-memory one (for example <c>c.UseSqlServerStorage(...)</c> from
    /// the Hangfire.SqlServer package). When set, <see cref="StorageType"/> is ignored, and the delegate must select a
    /// storage: otherwise Hangfire fails when the server starts ("JobStorage.Current ... not initialized").
    /// </summary>
    public Action<IGlobalConfiguration>? ConfigureStorage { get; set; }

    /// <summary>Number of worker threads.</summary>
    public int WorkerCount { get; set; } = Environment.ProcessorCount;
}
