using AwesomeAssertions;
using Kck.BackgroundJobs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Xunit;

namespace Kck.BackgroundJobs.Quartz.Tests;

public class QuartzJobExecutionTests
{
    private sealed class Signal
    {
        public TaskCompletionSource<bool> Fired { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class SignallingJob(Signal signal) : IBackgroundJob
    {
        public Task ExecuteAsync(CancellationToken ct = default)
        {
            signal.Fired.TrySetResult(true);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task EnqueueAsync_WithRealScheduler_ExecutesJobThroughDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Signal>();
        services.AddScoped<SignallingJob>();
        services.AddKckBackgroundJobsQuartz();

        await using var provider = services.BuildServiceProvider();
        var scheduler = await provider.GetRequiredService<ISchedulerFactory>().GetScheduler();
        await scheduler.Start();
        try
        {
            await provider.GetRequiredService<IJobScheduler>().EnqueueAsync<SignallingJob>();

            var fired = await provider.GetRequiredService<Signal>().Fired.Task.WaitAsync(TimeSpan.FromSeconds(10));

            fired.Should().BeTrue();
        }
        finally
        {
            await scheduler.Shutdown(waitForJobsToComplete: true);
        }
    }
}
