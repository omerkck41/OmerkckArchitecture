using FluentAssertions;
using Kck.BackgroundJobs.Abstractions;
using Kck.BackgroundJobs.Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.BackgroundJobs.Hangfire.Tests;

public sealed class HangfireJobSchedulerTests
{
    [Fact]
    public void AddKckBackgroundJobsHangfire_RegistersScheduler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckBackgroundJobsHangfire();

        var provider = services.BuildServiceProvider();
        var scheduler = provider.GetService<IJobScheduler>();
        scheduler.Should().NotBeNull();
        scheduler.Should().BeOfType<HangfireJobScheduler>();
    }

    [Fact]
    public void CronExpressions_HaveExpectedValues()
    {
        CronExpressions.EveryMinute.Should().Be("* * * * *");
        CronExpressions.EveryDay.Should().Be("0 0 * * *");
        CronExpressions.EveryHour.Should().Be("0 * * * *");
        CronExpressions.Every5Minutes.Should().Be("*/5 * * * *");
        CronExpressions.EveryWeek.Should().Be("0 0 * * 1");
        CronExpressions.EveryMonth.Should().Be("0 0 1 * *");
    }
}
