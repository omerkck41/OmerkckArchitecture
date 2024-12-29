namespace Core.ToolKit.Utilities;

public static class TimeRangeHelper
{
    /// <summary>
    /// Checks if two time ranges overlap.
    /// </summary>
    /// <param name="start1">Start time of the first range.</param>
    /// <param name="end1">End time of the first range.</param>
    /// <param name="start2">Start time of the second range.</param>
    /// <param name="end2">End time of the second range.</param>
    /// <returns>True if the ranges overlap, otherwise false.</returns>
    public static bool DoRangesOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        if (end1 <= start1 || end2 <= start2)
            throw new ArgumentException("End time must be after start time.");

        return start1 < end2 && start2 < end1;
    }

    /// <summary>
    /// Divides a time range into equal intervals with metadata for large-scale operations.
    /// </summary>
    /// <param name="start">Start time of the range.</param>
    /// <param name="end">End time of the range.</param>
    /// <param name="intervalMinutes">Length of each interval in minutes.</param>
    /// <returns>List of time intervals with metadata.</returns>
    public static List<(DateTime Start, DateTime End, int IntervalIndex)> SplitRangeWithMetadata(DateTime start, DateTime end, int intervalMinutes)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time.");

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
}