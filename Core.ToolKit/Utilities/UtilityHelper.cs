using System.Diagnostics;

namespace Core.ToolKit.Utilities;

public static class UtilityHelper
{
    public static async Task<T> RetryWithExponentialBackoffAsync<T>(Func<Task<T>> operation, int retryCount, int initialDelayMilliseconds, int maxDelayMilliseconds = int.MaxValue)
    {
        if (retryCount <= 0) throw new ArgumentException("Retry count must be greater than zero.", nameof(retryCount));

        var delay = initialDelayMilliseconds;

        for (var attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                return await operation();
            }
            catch
            {
                if (attempt == retryCount) throw;

                await Task.Delay(Math.Min(delay, maxDelayMilliseconds));
                delay *= 2; // Exponential backoff
            }
        }

        throw new InvalidOperationException("Retry operation failed.");
    }

    public static async Task<(long ExecutionTime, T Result)> MeasureExecutionTimeAsync<T>(Func<Task<T>> operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));

        var stopwatch = Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();

        return (stopwatch.ElapsedMilliseconds, result);
    }
}