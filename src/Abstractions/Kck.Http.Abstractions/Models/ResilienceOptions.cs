namespace Kck.Http.Abstractions;

/// <summary>
/// Configuration options for HTTP resilience policies, covering retry back-off, circuit breaker, and per-request timeout.
/// </summary>
public sealed class ResilienceOptions
{
    /// <summary>Gets or sets the maximum number of retry attempts before the request is considered failed. Defaults to 3.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Gets or sets the median delay before the first retry, used as the base for exponential back-off. Defaults to 1 second.</summary>
    public TimeSpan MedianFirstRetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Gets or sets how long the circuit breaker remains open after tripping. Defaults to 30 seconds.</summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Gets or sets the number of consecutive failures required to trip the circuit breaker. Defaults to 5.</summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>Gets or sets the maximum time allowed for a single HTTP request before it is cancelled. Defaults to 30 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
