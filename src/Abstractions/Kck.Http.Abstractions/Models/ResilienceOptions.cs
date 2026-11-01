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

    /// <summary>
    /// Gets or sets the share of failed requests in the sampling window that opens the circuit (0 to 1). Defaults to 0.1,
    /// the standard resilience handler's default.
    /// </summary>
    public double CircuitBreakerFailureRatio { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the minimum number of requests in the sampling window before the circuit can open. Defaults to 100,
    /// the standard resilience handler's default.
    /// </summary>
    public int CircuitBreakerMinimumThroughput { get; set; } = 100;

    /// <summary>Gets or sets the maximum time allowed for a single HTTP request before it is cancelled. Defaults to 30 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the JSON options for request bodies and responses. Native AOT apps set options whose
    /// <c>TypeInfoResolver</c> is a source-generated <c>JsonSerializerContext</c> covering every body and response type.
    /// <c>null</c> uses the web defaults (camelCase) with reflection-based metadata.
    /// </summary>
    public System.Text.Json.JsonSerializerOptions? SerializerOptions { get; set; }
}
