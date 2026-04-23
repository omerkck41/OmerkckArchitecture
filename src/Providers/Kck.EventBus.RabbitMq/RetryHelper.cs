namespace Kck.EventBus.RabbitMq;

internal static class RetryHelper
{
    public static TimeSpan CalculateDelay(int attempt, TimeSpan baseDelay, TimeSpan maxDelay)
    {
        var exponentialSeconds = baseDelay.TotalSeconds * Math.Pow(2, attempt);
        var cappedSeconds = Math.Min(exponentialSeconds, maxDelay.TotalSeconds);
        return TimeSpan.FromSeconds(cappedSeconds);
    }
}
