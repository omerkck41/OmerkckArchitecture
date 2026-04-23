namespace Kck.Http.Abstractions;

public sealed class ResilienceOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan MedianFirstRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
