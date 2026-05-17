namespace Kck.EventBus.RabbitMq;

/// <summary>
/// Utility class that calculates exponential back-off delays for RabbitMQ retry logic.
/// </summary>
internal static class RetryHelper
{
    /// <summary>
    /// Returns the back-off delay for the given <paramref name="attempt"/> using an exponential
    /// formula capped at <paramref name="maxDelay"/>.
    /// </summary>
    public static TimeSpan CalculateDelay(int attempt, TimeSpan baseDelay, TimeSpan maxDelay)
    {
        var exponentialSeconds = baseDelay.TotalSeconds * Math.Pow(2, attempt);
        var cappedSeconds = Math.Min(exponentialSeconds, maxDelay.TotalSeconds);
        return TimeSpan.FromSeconds(cappedSeconds);
    }
}
