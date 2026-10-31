using System.Globalization;

namespace Kck.BackgroundJobs.Quartz;

/// <summary>
/// Converts a 5-field crontab expression (minute hour day-of-month month day-of-week, as in <c>CronExpressions</c> and
/// Hangfire) to Quartz's format: a seconds field first, '?' in the unused day field, and days of week numbered from
/// Sunday = 1 (crontab: Sunday = 0 or 7). 6- and 7-field expressions are Quartz already and are returned unchanged.
/// </summary>
internal static class CrontabConverter
{
    public static string ToQuartz(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        var fields = expression.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 5)
            return expression;

        var (minute, hour, dayOfMonth, month, dayOfWeek) = (fields[0], fields[1], fields[2], fields[3], fields[4]);
        if (dayOfWeek is "*" or "?")
            return $"0 {minute} {hour} {(dayOfMonth == "?" ? "*" : dayOfMonth)} {month} ?";

        // crontab fires when either day field matches; Quartz needs one of them to be '?'.
        if (dayOfMonth is not ("*" or "?"))
        {
            throw new ArgumentException(
                $"Cron expression '{expression}' restricts both the day of month and the day of week. Quartz supports " +
                "one of them; use two schedules, or write a Quartz expression (6 or 7 fields).", nameof(expression));
        }

        return $"0 {minute} {hour} ? {month} {ToQuartzDaysOfWeek(dayOfWeek, expression)}";
    }

    // Names and numbers are expanded to the days they select and renumbered. L and # are not converted: crontab
    // dialects and Quartz number those days differently.
    private static string ToQuartzDaysOfWeek(string field, string expression)
    {
        var days = new SortedSet<int>();
        foreach (var part in field.Split(','))
            AddDays(part, days, expression);

        return days.Count == 7 ? "*" : string.Join(',', days.Select(day => day + 1));
    }

    // One list element: N, A-B, *, */S or A-B/S, with crontab days 0-7 (7 = Sunday = 0) or SUN-SAT.
    private static void AddDays(string part, SortedSet<int> days, string expression)
    {
        var (range, step) = part.Split('/') switch
        {
            [var r] => (r, 1),
            [var r, var s] when int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n > 0 => (r, n),
            _ => throw Invalid(expression),
        };

        var (first, last) = range switch
        {
            "*" => (0, 6),
            _ when range.Split('-') is [var a, var b] => (Day(a, expression), Day(b, expression)),
            _ => (Day(range, expression), part.Contains('/', StringComparison.Ordinal) ? 7 : Day(range, expression)),
        };

        if (first > last)
            throw Invalid(expression);

        for (var day = first; day <= last; day += step)
            days.Add(day % 7);
    }

    private static readonly string[] DayNames = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];

    private static int Day(string value, string expression)
    {
        if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var day) && day <= 7)
            return day;

        var name = Array.FindIndex(DayNames, n => n.Equals(value, StringComparison.OrdinalIgnoreCase));
        return name >= 0 ? name : throw Invalid(expression);
    }

    private static ArgumentException Invalid(string expression) =>
        new($"Cron expression '{expression}' has a day-of-week field that cannot be converted to Quartz (crontab days " +
            "0-7 or SUN-SAT, with lists, ranges and steps; for L or #, write a Quartz expression).", nameof(expression));
}
