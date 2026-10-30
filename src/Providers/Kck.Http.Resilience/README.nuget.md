# Kck.Http.Resilience

Microsoft.Extensions.Http.Resilience-based HTTP resilience pipelines — retry, circuit breaker, and timeout for `HttpClient` registrations.

## Installation

```bash
dotnet add package Kck.Http.Resilience
```

## Quick Start

```csharp
using Kck.Http.Abstractions;

// Program.cs - IApiClient as a typed HttpClient with the standard resilience handler
builder.Services.AddKckHttpResilience(options =>
{
    options.MaxRetryAttempts = 3;
    options.MedianFirstRetryDelay = TimeSpan.FromMilliseconds(500);
    options.Timeout = TimeSpan.FromSeconds(10);   // per attempt; the whole call gets 3x
});

// Usage (absolute URLs: the client has no base address)
public sealed record PaymentStatus(string Id, string State);

public class PaymentClient(IApiClient api)
{
    public async Task<PaymentStatus?> GetStatusAsync(string id, CancellationToken ct) =>
        (await api.GetAsync<PaymentStatus>($"https://payments.example.com/v1/payments/{id}", ct)).Data;
}
```

## Configuration

`ResilienceOptions` (Kck.Http.Abstractions):

| Property | Description | Default |
|---|---|---|
| `Timeout` | Attempt timeout; the whole call gets 3x | 30 s |
| `MaxRetryAttempts` | Retries on transient failures, exponential backoff | `3` |
| `MedianFirstRetryDelay` | Base retry delay | 1 s |
| `CircuitBreakerDuration` | How long the circuit stays open (4.7.0+; before, it set the sampling window) | 30 s |
| `CircuitBreakerFailureRatio` | Failure share that opens the circuit (4.7.0+) | `0.1` |
| `CircuitBreakerMinimumThroughput` | Requests in the window before it can open (4.7.0+) | `100` |
| `SerializerOptions` | JSON options for bodies (source-generated for Native AOT) | web defaults |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/http.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
