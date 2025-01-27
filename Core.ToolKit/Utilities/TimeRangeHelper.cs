namespace Core.ToolKit.Utilities;

public static class TimeRangeHelper
{
    public static bool DoRangesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        ValidateTimeRange(start1, end1);
        ValidateTimeRange(start2, end2);

        return start1 < end2 && start2 < end1;
    }

    public static List<(DateTime Start, DateTime End, int IntervalIndex)> SplitRangeWithMetadata(DateTime start, DateTime end, int intervalMinutes)
    {
        ValidateTimeRange(start, end);

        if (intervalMinutes <= 0)
            throw new ArgumentException("Interval must be greater than zero.", nameof(intervalMinutes));

        var result = new List<(DateTime Start, DateTime End, int IntervalIndex)>();
        var current = start;
        var index = 0;

        while (current < end)
        {
            var next = current.AddMinutes(intervalMinutes);
            result.Add((current, next < end ? next : end, index++));
            current = next;
        }

        return result;
    }

    private static void ValidateTimeRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time.");
    }
}