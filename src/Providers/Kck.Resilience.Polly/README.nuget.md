# Kck.Resilience.Polly

Polly v8 resilience pipeline provider for Kck applications — retry, circuit breaker, timeout, and hedging strategies via named pipelines.

## Installation

```bash
dotnet add package Kck.Resilience.Polly
```

## Quick Start

```csharp
using Kck.Resilience.Polly;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;

// Program.cs - a named pipeline (Polly v8)
builder.Services.AddKckResilience("api", pipeline => pipeline
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(300),
        UseJitter = true
    })
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(10)
    })
    .AddTimeout(TimeSpan.FromSeconds(5)));

// Resolve and use the pipeline
public class ExternalApiService(ResiliencePipelineProvider<string> pipelines, HttpClient http)
{
    public async Task<string> CallAsync(CancellationToken ct)
    {
        var pipeline = pipelines.GetPipeline("api");
        return await pipeline.ExecuteAsync(async token => await http.GetStringAsync("https://api.example.com/data", token), ct);
    }
}
```

## Configuration

No options of its own: `AddKckResilience(name, builder => ...)` registers a named Polly v8 pipeline; configure strategies with Polly's options (`RetryStrategyOptions`, `CircuitBreakerStrategyOptions`, `AddTimeout`). Resolve it with `ResiliencePipelineProvider<string>.GetPipeline(name)`.

## Scope

This package is a thin, name-consistent layer over Microsoft.Extensions.Resilience / Polly v8 with no abstraction of
its own: `AddKckResilience(name, ...)` is `AddResiliencePipeline(name, ...)` and `AddKckResilience<TResult>` is
`AddResiliencePipeline<string, TResult>`. For anything beyond that (a non-string key, the overload with the
`AddResiliencePipelineContext` for options reload, `AddResilienceEnricher` telemetry) call `AddResiliencePipeline`
directly and resolve through `ResiliencePipelineProvider<string>`; pipelines registered either way live in the same
registry.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/http.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
