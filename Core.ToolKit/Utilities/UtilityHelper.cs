using System.Diagnostics;

namespace Core.ToolKit.Utilities;

public static class UtilityHelper
{
    /// <summary>
    /// Retries a task a specified number of times with exponential backoff.
    /// </summary>
    /// <typeparam name="T">The return type of the task.</typeparam>
    /// <param name="operation">The task to retry.</param>
    /// <param name="retryCount">Number of retry attempts.</param>
    /// <param name="initialDelayMilliseconds">Initial delay between retries in milliseconds.</param>
    /// <returns>The result of the task.</returns>
    public static async Task<T> RetryWithExponentialBackoffAsync<T>(Func<Task<T>> operation, int retryCount, int initialDelayMilliseconds)
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

                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }
        }

        throw new InvalidOperationException("Retry operation failed.");
    }

    /// <summary>
    /// Measures the execution time of an asynchronous task.
    /// </summary>
    /// <typeparam name="T">The return type of the task.</typeparam>
    /// <param name="operation">The asynchronous task to measure.</param>
    /// <returns>Tuple containing the execution time in milliseconds and the task result.</returns>
    public static async Task<(long ExecutionTime, T Result)> MeasureExecutionTimeAsync<T>(Func<Task<T>> operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));

        var stopwatch = Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();

        return (stopwatch.ElapsedMilliseconds, result);
    }
}