# HTTP Resilience

A retry + circuit breaker + timeout wrapper for external service calls.
Built on Polly (Microsoft.Extensions.Http.Resilience).

## Packages

| Package | Target |
|---|---|
| `Kck.Http.Abstractions` | `IApiClient`, `ApiResponse<T>` |
| `Kck.Http.Resilience` | Polly-based `ResilientApiClient` |

## Setup

```csharp
services.AddKckHttpResilience(opt =>
{
    opt.Timeout = TimeSpan.FromSeconds(30);                    // per attempt; the whole call gets 3x
    opt.MaxRetryAttempts = 3;
    opt.MedianFirstRetryDelay = TimeSpan.FromSeconds(1);
    opt.CircuitBreakerDuration = TimeSpan.FromSeconds(30);         // how long the circuit stays open
    opt.CircuitBreakerFailureRatio = 0.1;                          // share of failures that opens it
    opt.CircuitBreakerMinimumThroughput = 100;                     // requests needed before it can open
});
```

`AddKckHttpResilience` registers `IApiClient` as a typed `HttpClient` with the standard resilience handler of
`Microsoft.Extensions.Http.Resilience` (rate limiter, total timeout, retry, circuit breaker, attempt timeout). The
client has no base address: pass absolute URLs.

## Usage

```csharp
using Kck.Http.Abstractions;

public class UpstreamService(IApiClient client)
{
    public Task<ApiResponse<Product>> GetProductAsync(int id, CancellationToken ct) =>
        client.GetAsync<Product>($"https://api.upstream.com/products/{id}", ct);

    public Task<ApiResponse<Order>> PostOrderAsync(Order order, CancellationToken ct) =>
        client.PostAsync<Order>("https://api.upstream.com/orders", order, ct);
}
```

`SendAsync<T>(method, url, body, headers, ct)` adds request headers. Bodies are serialized with `System.Text.Json`
(`ResilienceOptions.SerializerOptions`, or web defaults).

## Resilience Policies

| Option | Maps to |
|---|---|
| `Timeout` (30 s) | attempt timeout; the total request timeout is 3 x `Timeout` |
| `MaxRetryAttempts` (3) | retries on transient failures (5xx, 408, 429, network errors) with exponential backoff |
| `MedianFirstRetryDelay` (1 s) | the base retry delay |
| `CircuitBreakerDuration` (30 s) | the break duration: how long the circuit stays open |
| `CircuitBreakerFailureRatio` (0.1) | share of failed requests in the sampling window that opens the circuit |
| `CircuitBreakerMinimumThroughput` (100) | requests in the sampling window before the circuit can open |

The sampling window is 2 x `Timeout`, at least 30 s (the standard handler requires at least twice the attempt timeout).
Invalid values (for example a failure ratio above 1) fail the standard handler's options validation at startup.
5.0 removed `CircuitBreakerFailureThreshold` (obsolete since 4.7.0, KCK0900: it was never applied). Before 4.7.0, `CircuitBreakerDuration`
set the sampling window (2 x the value) and the break duration was the handler's default (5 s).

## ApiResponse\<T>

```csharp
public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
}
```

- HTTP 2xx: `IsSuccess = true`, `Data` is the deserialized body.
- Any other status (after retries): `IsSuccess = false`, `ErrorMessage` is the response body, and the failure is logged.
- Exceptions from the resilience pipeline (an open circuit, the attempt or total timeout after the last retry) and
  network errors are not converted: they propagate to the caller. With the defaults, the total timeout (90 s) fires
  before `HttpClient`'s own 100 s timeout, so expect `TimeoutRejectedException` rather than a response.
- Only an `HttpClient` timeout (a `TaskCanceledException` not caused by your cancellation token) becomes
  `IsSuccess = false` with status 408.

## Several Upstream Services

`AddKckHttpResilience` registers a single `IApiClient`. For several upstreams with their own base address or policy,
register your own typed clients with `services.AddHttpClient<TClient>(...)` and `.AddStandardResilienceHandler(...)`.
