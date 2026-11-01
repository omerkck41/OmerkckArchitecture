using Hangfire;
using Kck.BackgroundJobs.Abstractions;

namespace Kck.BackgroundJobs.Hangfire;

/// <summary>
/// <see cref="IJobScheduler"/> on Hangfire. Every job is stored as a call to
/// <see cref="HangfireJobRunner{TJob}.RunAsync"/>, which Hangfire activates per execution and which resolves the
/// job from DI. Writes through the <see cref="IBackgroundJobClient"/> and <see cref="IRecurringJobManager"/> that
/// Hangfire.NetCore registers in DI (on the <c>JobStorage</c> configured there).
/// </summary>
/// <remarks>
/// The <c>ct</c> parameter of every method is not observed and not stored with the job: the Hangfire calls are
/// synchronous storage writes that complete before the method returns, and the token a job receives at execution is
/// the one Hangfire supplies to <see cref="HangfireJobRunner{TJob}.RunAsync"/>.
/// </remarks>
public sealed class HangfireJobScheduler(IBackgroundJobClient client, IRecurringJobManager recurringJobs) : IJobScheduler
{
    /// <summary>Stores a fire-and-forget run of <typeparamref name="TJob"/> in the default queue.</summary>
    /// <param name="ct">Not observed and not forwarded to Hangfire (see the class remarks).</param>
    public Task EnqueueAsync<TJob>(CancellationToken ct = default) where TJob : IBackgroundJob
    {
        client.Enqueue<HangfireJobRunner<TJob>>(runner => runner.RunAsync(CancellationToken.None));
        return Task.CompletedTask;
    }

    /// <summary>Stores a run of <typeparamref name="TJob"/> that becomes due after <paramref name="delay"/>.</summary>
    /// <param name="delay">Time until the job is due.</param>
    /// <param name="ct">Not observed and not forwarded to Hangfire (see the class remarks).</param>
    public Task ScheduleAsync<TJob>(TimeSpan delay, CancellationToken ct = default) where TJob : IBackgroundJob
    {
        client.Schedule<HangfireJobRunner<TJob>>(runner => runner.RunAsync(CancellationToken.None), delay);
        return Task.CompletedTask;
    }

    /// <summary>Creates or replaces the recurring job <paramref name="jobId"/> running <typeparamref name="TJob"/>.</summary>
    /// <param name="jobId">Recurring job id; an existing job with this id is updated.</param>
    /// <param name="cronExpression">Crontab expression (5 fields).</param>
    /// <param name="ct">Not observed and not forwarded to Hangfire (see the class remarks).</param>
    public Task AddOrUpdateRecurringAsync<TJob>(string jobId, string cronExpression, CancellationToken ct = default)
        where TJob : IBackgroundJob
    {
        recurringJobs.AddOrUpdate<HangfireJobRunner<TJob>>(
            jobId, runner => runner.RunAsync(CancellationToken.None), cronExpression);
        return Task.CompletedTask;
    }

    /// <summary>Removes the recurring job <paramref name="jobId"/> if it exists.</summary>
    /// <param name="jobId">Recurring job id.</param>
    /// <param name="ct">Not observed (see the class remarks).</param>
    public Task RemoveRecurringAsync(string jobId, CancellationToken ct = default)
    {
        recurringJobs.RemoveIfExists(jobId);
        return Task.CompletedTask;
    }
}
