using AwesomeAssertions;
using Hangfire;
using Hangfire.InMemory;
using Hangfire.Storage;
using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Kck.BackgroundJobs.Hangfire.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class HangfireStaticState
{
    // GlobalConfiguration and JobStorage.Current are process-wide statics: these tests never run in parallel.
    public const string Name = "Hangfire";
}

/// <summary>
/// The scheduler against a real (in-memory) Hangfire storage: what reaches the storage, and an end-to-end run by a
/// Hangfire server.
/// </summary>
[Collection(HangfireStaticState.Name)]
public sealed class HangfireJobStorageTests
{
    [Fact]
    public async Task AddOrUpdateRecurringAsync_RegistersJobWithCronInStorage()
    {
        using var storage = new InMemoryStorage();
        var scheduler = CreateProvider(storage).GetRequiredService<IJobScheduler>();

        await scheduler.AddOrUpdateRecurringAsync<TestJob>("nightly", CronExpressions.EveryDay);

        using var connection = storage.GetConnection();
        var job = connection.GetRecurringJobs().Should().ContainSingle().Subject;
        job.Id.Should().Be("nightly");
        job.Cron.Should().Be(CronExpressions.EveryDay);
        job.Job.Type.Should().Be<HangfireJobRunner<TestJob>>();
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_SameId_UpdatesCron()
    {
        using var storage = new InMemoryStorage();
        var scheduler = CreateProvider(storage).GetRequiredService<IJobScheduler>();

        await scheduler.AddOrUpdateRecurringAsync<TestJob>("report", CronExpressions.EveryDay);
        await scheduler.AddOrUpdateRecurringAsync<TestJob>("report", CronExpressions.EveryHour);

        using var connection = storage.GetConnection();
        var job = connection.GetRecurringJobs().Should().ContainSingle().Subject;
        job.Cron.Should().Be(CronExpressions.EveryHour);
    }

    [Fact]
    public async Task RemoveRecurringAsync_RemovesTheJob()
    {
        using var storage = new InMemoryStorage();
        var scheduler = CreateProvider(storage).GetRequiredService<IJobScheduler>();
        await scheduler.AddOrUpdateRecurringAsync<TestJob>("cleanup", CronExpressions.EveryHour);

        await scheduler.RemoveRecurringAsync("cleanup");

        using var connection = storage.GetConnection();
        connection.GetRecurringJobs().Should().BeEmpty();
    }

    [Fact]
    public async Task EnqueueAsync_EnqueuesRunnerJob()
    {
        using var storage = new InMemoryStorage();
        var scheduler = CreateProvider(storage).GetRequiredService<IJobScheduler>();

        await scheduler.EnqueueAsync<TestJob>();

        var monitoring = storage.GetMonitoringApi();
        monitoring.EnqueuedCount("default").Should().Be(1);
        var enqueued = monitoring.EnqueuedJobs("default", 0, 10).Should().ContainSingle().Subject;
        enqueued.Value.Job.Type.Should().Be<HangfireJobRunner<TestJob>>();
    }

    [Fact]
    public async Task ScheduleAsync_CreatesScheduledJob()
    {
        using var storage = new InMemoryStorage();
        var scheduler = CreateProvider(storage).GetRequiredService<IJobScheduler>();

        await scheduler.ScheduleAsync<TestJob>(TimeSpan.FromHours(1));

        storage.GetMonitoringApi().ScheduledCount().Should().Be(1);
    }

    [Fact]
    public async Task HangfireJobRunner_RunAsync_ExecutesGivenJob()
    {
        var signal = new JobSignal();
        var runner = new HangfireJobRunner<TestJob>(new TestJob(signal));

        await runner.RunAsync(CancellationToken.None);

        signal.Executed.Task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task EnqueuedJob_IsExecutedByServer()
    {
        using var storage = new InMemoryStorage();
        var signal = new JobSignal();
        var provider = CreateProvider(storage, signal, workerCount: 1);
        var hostedServices = provider.GetServices<IHostedService>().ToList();
        foreach (var hostedService in hostedServices)
            await hostedService.StartAsync(CancellationToken.None);

        try
        {
            await provider.GetRequiredService<IJobScheduler>().EnqueueAsync<TestJob>();

            var completed = await Task.WhenAny(signal.Executed.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            completed.Should().BeSameAs(signal.Executed.Task, "the Hangfire server should run the enqueued job");
        }
        finally
        {
            foreach (var hostedService in hostedServices)
                await hostedService.StopAsync(CancellationToken.None);
        }
    }

    // Not disposed: Hangfire keeps the configured logger in static state shared by the tests.
    private static ServiceProvider CreateProvider(InMemoryStorage storage, JobSignal? signal = null, int workerCount = 1)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(signal ?? new JobSignal());
        services.AddKckJob<TestJob>();
        services.AddKckBackgroundJobsHangfire(o =>
        {
            o.ConfigureStorage = config => config.UseStorage(storage);
            o.WorkerCount = workerCount;
        });
        return services.BuildServiceProvider();
    }
}

public sealed class JobSignal
{
    public TaskCompletionSource Executed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public sealed class TestJob(JobSignal signal) : IBackgroundJob
{
    public Task ExecuteAsync(CancellationToken ct = default)
    {
        signal.Executed.TrySetResult();
        return Task.CompletedTask;
    }
}
