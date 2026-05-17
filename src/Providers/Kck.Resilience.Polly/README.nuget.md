# Kck.Resilience.Polly

Polly v8 resilience pipeline provider for Kck applications — retry, circuit breaker, timeout, and hedging strategies via named pipelines.

## Installation

```bash
dotnet add package Kck.Resilience.Polly
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckResilience("api", pipeline =>
{
    pipeline.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(300),
        UseJitter = true
    });
    pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(10)
    });
    pipeline.AddTimeout(TimeSpan.FromSeconds(5));
});

// Resolve and use the pipeline
public class ExternalApiService(ResiliencePipelineProvider<string> pipelines)
{
    public async Task<string> CallAsync(CancellationToken ct)
    {
        var pipeline = pipelines.GetPipeline("api");
        return await pipeline.ExecuteAsync(async t => await _http.GetStringAsync("/data", t), ct);
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| Pipeline name | Key for `GetPipeline(name)` | required |
| `MaxRetryAttempts` | Retry count | `3` |
| `FailureRatio` | Circuit breaker open threshold | `0.5` |
| `TimeoutInterval` | Per-execution timeout | `30s` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/http.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
