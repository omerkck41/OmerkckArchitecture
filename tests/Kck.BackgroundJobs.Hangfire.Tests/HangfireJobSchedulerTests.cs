using AwesomeAssertions;
using Hangfire;
using Kck.BackgroundJobs.Abstractions;
using Kck.BackgroundJobs.Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.BackgroundJobs.Hangfire.Tests;

public sealed class HangfireJobSchedulerTests
{
    [Fact]
    public void AddKckBackgroundJobsHangfire_ConfigureStorage_ReplacesTheInMemoryStorage()
    {
        // A durable storage (Hangfire.SqlServer, Hangfire.PostgreSql...) is plugged in through ConfigureStorage.
        using var storage = new global::Hangfire.InMemory.InMemoryStorage();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckBackgroundJobsHangfire(o => o.ConfigureStorage = config => config.UseStorage(storage));

        // Not disposed: Hangfire keeps the configured logger in static state shared by the tests.
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<global::Hangfire.JobStorage>().Should().BeSameAs(storage);
    }

    [Fact]
    public void AddKckBackgroundJobsHangfire_UnknownStorageTypeWithoutConfigureStorage_Throws()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckBackgroundJobsHangfire(o => o.StorageType = "SqlServer");

        // Not disposed: Hangfire keeps the configured logger in static state shared by the tests.
        var provider = services.BuildServiceProvider();
        var resolve = () => provider.GetRequiredService<global::Hangfire.JobStorage>();

        resolve.Should().Throw<NotSupportedException>().WithMessage("*ConfigureStorage*");
    }

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
