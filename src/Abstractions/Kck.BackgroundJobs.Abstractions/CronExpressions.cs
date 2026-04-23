namespace Kck.BackgroundJobs.Abstractions;

/// <summary>
/// Common cron expressions for recurring jobs.
/// </summary>
public static class CronExpressions
{
    public const string EveryMinute = "* * * * *";
    public const string Every5Minutes = "*/5 * * * *";
    public const string Every15Minutes = "*/15 * * * *";
    public const string EveryHour = "0 * * * *";
    public const string EveryDay = "0 0 * * *";
    public const string EveryWeek = "0 0 * * 1";
    public const string EveryMonth = "0 0 1 * *";
}
