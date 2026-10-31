using AwesomeAssertions;
using Kck.BackgroundJobs.Abstractions;
using NSubstitute;
using Quartz;
using Xunit;

namespace Kck.BackgroundJobs.Quartz.Tests;

/// <summary>
/// <see cref="CronExpressions"/> are 5-field crontab; Quartz needs seconds and '?' in one day field, and numbers its
/// days of week from Sunday = 1. The scheduler converts, so every constant schedules and fires when crontab would.
/// </summary>
public class QuartzCronConversionTests
{
    private sealed class TestJob : IBackgroundJob
    {
        public Task ExecuteAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    // Sunday 2026-11-01 10:30:15 UTC.
    private static readonly DateTimeOffset Start = new(2026, 11, 1, 10, 30, 15, TimeSpan.Zero);

    private static async Task<string> ScheduledCronAsync(string crontab)
    {
        var scheduler = Substitute.For<IScheduler>();
        var factory = Substitute.For<ISchedulerFactory>();
        factory.GetScheduler(Arg.Any<CancellationToken>()).Returns(scheduler);
        ITrigger? trigger = null;
        await scheduler.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Do<ITrigger>(t => trigger = t), Arg.Any<ScheduleJobOptions>(),
            Arg.Any<CancellationToken>());

        await new QuartzJobScheduler(factory).AddOrUpdateRecurringAsync<TestJob>("job", crontab);

        return trigger.Should().BeAssignableTo<ICronTrigger>().Which.CronExpressionString!;
    }

    private static DateTimeOffset NextAfter(string quartzCron, DateTimeOffset after) =>
        new CronExpression(quartzCron, TimeZoneInfo.Utc).GetNextValidTimeAfter(after)!.Value;

    [Theory]
    [InlineData(CronExpressions.EveryMinute, "2026-11-01T10:31:00Z")]
    [InlineData(CronExpressions.Every5Minutes, "2026-11-01T10:35:00Z")]
    [InlineData(CronExpressions.Every15Minutes, "2026-11-01T10:45:00Z")]
    [InlineData(CronExpressions.EveryHour, "2026-11-01T11:00:00Z")]
    [InlineData(CronExpressions.EveryDay, "2026-11-02T00:00:00Z")]
    [InlineData(CronExpressions.EveryWeek, "2026-11-02T00:00:00Z")] // Monday, as in crontab (1 = Monday)
    [InlineData(CronExpressions.EveryMonth, "2026-12-01T00:00:00Z")]
    public async Task AddOrUpdateRecurringAsync_CronExpressionsConstant_FiresAsCrontab(string crontab, string expectedNext)
    {
        var quartz = await ScheduledCronAsync(crontab);

        NextAfter(quartz, Start).Should().Be(DateTimeOffset.Parse(expectedNext, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData("0 9 * * 1-5", "2026-11-02T09:00:00Z")] // weekdays: Monday next
    [InlineData("0 9 * * 0", "2026-11-08T09:00:00Z")]   // Sunday (0)
    [InlineData("0 9 * * 7", "2026-11-08T09:00:00Z")]   // Sunday (7)
    [InlineData("0 9 * * 6,0", "2026-11-07T09:00:00Z")] // Saturday and Sunday
    [InlineData("0 9 * * MON", "2026-11-02T09:00:00Z")] // names are the same in both
    [InlineData("0 9 15 * *", "2026-11-15T09:00:00Z")]  // day of month
    [InlineData("0 9 15 * ?", "2026-11-15T09:00:00Z")]  // '?' leaves the day of week unrestricted
    [InlineData("0 9 ? * *", "2026-11-02T09:00:00Z")]
    [InlineData("0 9 * * SAT,3", "2026-11-04T09:00:00Z")] // names next to numbers: 3 is still Wednesday
    [InlineData("0 9 * * 3-FRI", "2026-11-04T09:00:00Z")]
    [InlineData("0 9 * * */2", "2026-11-03T09:00:00Z")]   // Sunday, Tuesday, Thursday, Saturday
    [InlineData("0 9 * * 0-7", "2026-11-02T09:00:00Z")]
    [InlineData("0\t9 * * 3", "2026-11-04T09:00:00Z")]
    public async Task AddOrUpdateRecurringAsync_Crontab_FiresAsCrontab(string crontab, string expectedNext)
    {
        var quartz = await ScheduledCronAsync(crontab);

        NextAfter(quartz, Start).Should().Be(DateTimeOffset.Parse(expectedNext, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_QuartzExpression_IsUsedAsIs()
    {
        (await ScheduledCronAsync("0 0 6 ? * MON-FRI")).Should().Be("0 0 6 ? * MON-FRI");
    }

    [Fact]
    public async Task AddOrUpdateRecurringAsync_CrontabWithDayOfMonthAndDayOfWeek_ThrowsArgumentException()
    {
        // crontab runs when either matches; Quartz cannot express that.
        var act = () => ScheduledCronAsync("0 9 15 * 1");

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*day of month*day of week*");
    }

    [Theory]
    [InlineData("0 9 * * 5L")]  // last Friday: crontab and Quartz number it differently
    [InlineData("0 9 * * 1#2")] // second Monday
    [InlineData("0 9 * * 8")]
    [InlineData("0 9 * * 5-1")]
    [InlineData("0 9 * * MOX")]
    public async Task AddOrUpdateRecurringAsync_DayOfWeekNotConvertible_ThrowsArgumentException(string crontab)
    {
        var act = () => ScheduledCronAsync(crontab);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*day-of-week*");
    }
}
