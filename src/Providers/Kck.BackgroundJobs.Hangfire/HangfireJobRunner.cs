using Hangfire;
using Kck.BackgroundJobs.Abstractions;

namespace Kck.BackgroundJobs.Hangfire;

/// <summary>
/// The method Hangfire stores and later invokes for a Kck job. Hangfire activates the runner through its job
/// activator (a DI scope per execution with Hangfire.NetCore), which resolves <typeparamref name="TJob"/> from
/// that scope — register the job with <c>AddKckJob&lt;TJob&gt;()</c>.
/// </summary>
/// <typeparam name="TJob">The job to run.</typeparam>
public sealed class HangfireJobRunner<TJob>(TJob job) where TJob : IBackgroundJob
{
    /// <summary>Runs the job. Failed runs are retried up to three times.</summary>
    /// <param name="ct">
    /// Supplied by Hangfire at execution time, not by the caller: Hangfire replaces every
    /// <see cref="CancellationToken"/> argument with its own token, which is cancelled when the server shuts down
    /// or the job is deleted. The <see cref="CancellationToken.None"/> written at enqueue time is only a placeholder.
    /// </param>
    [AutomaticRetry(Attempts = 3)]
    public Task RunAsync(CancellationToken ct) => job.ExecuteAsync(ct);
}
