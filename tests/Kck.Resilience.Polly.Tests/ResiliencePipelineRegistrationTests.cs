using AwesomeAssertions;
using Kck.Resilience.Polly;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using Xunit;

namespace Kck.Resilience.Polly.Tests;

/// <summary>
/// The package is a thin, name-consistent layer over <c>AddResiliencePipeline</c>: these tests pin that a pipeline
/// registered through it resolves by name and runs the configured Polly v8 strategies.
/// </summary>
public sealed class ResiliencePipelineRegistrationTests
{
    private static ResiliencePipelineProvider<string> Build(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        return services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();
    }

    [Fact]
    public void AddKckResilience_RegistersNamedPipeline_ResolvableByName()
    {
        var pipelines = Build(s => s.AddKckResilience("api", b => b.AddTimeout(TimeSpan.FromSeconds(5))));

        var pipeline = pipelines.GetPipeline("api");

        pipeline.Should().NotBeNull();
        pipeline.Should().NotBeSameAs(ResiliencePipeline.Empty);
    }

    [Fact]
    public void GetPipeline_UnknownName_ThrowsKeyNotFoundException()
    {
        var pipelines = Build(s => s.AddKckResilience("api", b => b.AddTimeout(TimeSpan.FromSeconds(5))));

        var resolve = () => pipelines.GetPipeline("missing");

        resolve.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public async Task Retry_MaxRetryAttempts3_ExecutesFourTimesThenThrows()
    {
        var pipelines = Build(s => s.AddKckResilience("retry", b => b.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.Zero,
            ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
        })));
        var attempts = 0;

        var execute = async () => await pipelines.GetPipeline("retry").ExecuteAsync(_ =>
        {
            attempts++;
            throw new InvalidOperationException("boom");
        });

        await execute.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(4);
    }

    [Fact]
    public async Task CircuitBreaker_AfterMinimumThroughputFailures_ThrowsBrokenCircuitException()
    {
        var pipelines = Build(s => s.AddKckResilience("breaker", b => b.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 2,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
        })));
        var pipeline = pipelines.GetPipeline("breaker");
        static ValueTask Fail(CancellationToken _) => throw new InvalidOperationException("boom");

        for (var i = 0; i < 2; i++)
        {
            var failing = async () => await pipeline.ExecuteAsync(Fail);
            await failing.Should().ThrowAsync<InvalidOperationException>();
        }

        var afterThreshold = async () => await pipeline.ExecuteAsync(_ => ValueTask.CompletedTask);
        await afterThreshold.Should().ThrowAsync<BrokenCircuitException>();
    }

    [Fact]
    public async Task Timeout_SlowCallback_ThrowsTimeoutRejectedException()
    {
        var pipelines = Build(s => s.AddKckResilience("timeout", b => b.AddTimeout(TimeSpan.FromMilliseconds(50))));

        var execute = async () => await pipelines.GetPipeline("timeout")
            .ExecuteAsync(async ct => await Task.Delay(TimeSpan.FromSeconds(30), ct));

        await execute.Should().ThrowAsync<TimeoutRejectedException>();
    }

    [Fact]
    public async Task AddKckResilienceTResult_ShouldHandleResult_RetriesOnPredicate()
    {
        var services = new ServiceCollection();
        services.AddKckResilience<int>("typed", b => b.AddRetry(new RetryStrategyOptions<int>
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.Zero,
            ShouldHandle = new PredicateBuilder<int>().HandleResult(r => r < 0),
        }));
        var pipelines = services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();
        var results = new Queue<int>([-1, -1, 42]);
        var calls = 0;

        var result = await pipelines.GetPipeline<int>("typed").ExecuteAsync(_ =>
        {
            calls++;
            return ValueTask.FromResult(results.Dequeue());
        });

        result.Should().Be(42);
        calls.Should().Be(3);
    }

    [Fact]
    public void AddKckResilience_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var untyped = services.AddKckResilience("a", b => b.AddTimeout(TimeSpan.FromSeconds(1)));
        var typed = services.AddKckResilience<int>("b", b => b.AddTimeout(TimeSpan.FromSeconds(1)));

        untyped.Should().BeSameAs(services);
        typed.Should().BeSameAs(services);
    }
}
