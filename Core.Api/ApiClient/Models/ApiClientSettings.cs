namespace Core.Api.ApiClient.Models;

// Bu sınıf, retry, circuit breaker ve timeout gibi ayarların dışarıdan konfigürasyona alınmasını sağlar.
public class ApiClientSettings
{
    public int RetryCount { get; set; } = 3;
    public double BaseDelaySeconds { get; set; } = 2;
    public int CircuitBreakerThreshold { get; set; } = 2;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 20;
}