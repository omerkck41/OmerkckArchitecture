using FluentAssertions;
using Kck.BackgroundJobs.Abstractions;
using Kck.BackgroundJobs.Quartz;
using NSubstitute;
using Quartz;
using Quartz.Impl.Matchers;
using Xunit;

namespace Kck.BackgroundJobs.Quartz.Tests;

public class QuartzJobSchedulerTests
{
    private sealed class TestJob : IBackgroundJob
    {
        public Task ExecuteAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private static (QuartzJobScheduler sut, IScheduler scheduler) CreateSut()
    {
        var scheduler = Substitute.For<IScheduler>();
        var factory = Substitute.For<ISchedulerFactory>();
        factory.GetScheduler(Arg.Any<CancellationToken>()).Returns(scheduler);
        return (new QuartzJobScheduler(factory), scheduler);
    }

    [Fact]
    public async Task EnqueueAsync_ShouldScheduleStartNowTrigger()
    {
        var (sut, scheduler) = CreateSut();

        await sut.EnqueueAsync<TestJob>();

        await scheduler.Received(1).ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Is<ITrigger>(t => t.StartTimeUtc <= DateTimeOffset.UtcNow.AddSeconds(1)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScheduleAsync_ShouldUseDelayedStartTime()
    {
        var (sut, scheduler) = CreateSut();

        await sut.ScheduleAsync<TestJob>(TimeSpan.FromMinutes(5));

        await scheduler.Received(1).ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Is<ITrigger>(t =>
                t.StartTimeUtc >= DateTimeOffset.UtcNow.AddMinutes(4) &&
                t.StartTimeUtc <= DateTimeOffset.UtcNow.AddMinutes(6)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_WhenJobNotExists_ShouldScheduleWithoutDelete()
    {
        var (sut, scheduler) = CreateSut();
        scheduler.CheckExists(Arg.Any<JobKey>(), Arg.Any<CancellationToken>()).Returns(false);

        await sut.AddOrUpdateRecurringAsync<TestJob>("daily-cleanup", "0 0 * * * ?");

        await scheduler.DidNotReceive().DeleteJob(Arg.Any<JobKey>(), Arg.Any<CancellationToken>());
        await scheduler.Received(1).ScheduleJob(
            Arg.Is<IJobDetail>(j => j.Key.Name == "daily-cleanup"),
            Arg.Any<ITrigger>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_WhenJobExists_ShouldDeleteBeforeReschedule()
    {
        var (sut, scheduler) = CreateSut();
        scheduler.CheckExists(Arg.Any<JobKey>(), Arg.Any<CancellationToken>()).Returns(true);

        await sut.AddOrUpdateRecurringAsync<TestJob>("daily-cleanup", "0 0 * * * ?");

        await scheduler.Received(1).DeleteJob(
            Arg.Is<JobKey>(k => k.Name == "daily-cleanup"),
            Arg.Any<CancellationToken>());
        await scheduler.Received(1).ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<ITrigger>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveRecurringAsync_ShouldDeleteByJobKey()
    {
        var (sut, scheduler) = CreateSut();

        await sut.RemoveRecurringAsync("daily-cleanup");

        await scheduler.Received(1).DeleteJob(
            Arg.Is<JobKey>(k => k.Name == "daily-cleanup"),
            Arg.Any<CancellationToken>());
    }
}
