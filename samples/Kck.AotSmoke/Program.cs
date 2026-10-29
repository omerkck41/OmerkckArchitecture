using System.Text.Json.Serialization;
using Kck.Caching.Abstractions;
using Kck.EventBus.Abstractions;
using Kck.FeatureFlags.Abstractions;
using Kck.Pipeline.Abstractions;
using Kck.Pipeline.Mediator;
using Kck.Pipeline.Mediator.Behaviors;
using Kck.Security.Abstractions.Hashing;
using Kck.Security.Abstractions.Mfa;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Exercises the AOT-compatible providers end to end; exit code 0 means every check passed.
var services = new ServiceCollection()
    .AddLogging(b => b.AddSimpleConsole())
    .AddDistributedMemoryCache()
    .AddKckCachingInMemory()
    .AddKckFeatureFlagsInMemory(o => o.Features["smoke"] = true)
    .AddKckArgon2(o =>
    {
        o.MemorySize = 19_456;
        o.Iterations = 2;
    })
    .AddKckTotp()
    .AddKckEventBus(b => b.UseInMemory())
    .AddSingleton<PingHandler>()
    .AddSingleton<IEventHandler<Ping>>(sp => sp.GetRequiredService<PingHandler>());
new KckMediatorPipelineBuilder(services)
    .UseCachingBehavior(o => o.SerializerOptions = SmokeJsonContext.Default.Options);

await using var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Kck.AotSmoke");
var failures = new List<string>();

void Check(bool passed, string name)
{
    if (passed)
    {
        SmokeLog.Passed(logger, name);
        return;
    }

    SmokeLog.Failed(logger, name);
    failures.Add(name);
}

var behavior = provider.GetServices<IPipelineBehavior<CountQuery, Count>>().OfType<CachingBehavior<CountQuery, Count>>().Single();
var first = await behavior.Handle(new CountQuery("count"), (_, _) => ValueTask.FromResult(new Count(1)), CancellationToken.None);
var second = await behavior.Handle(new CountQuery("count"), (_, _) => ValueTask.FromResult(new Count(2)), CancellationToken.None);
Check(first == second, "Pipeline.Mediator caching behavior (source-generated JSON)");

var cache = provider.GetRequiredService<ICacheService>();
await cache.SetAsync("greeting", "hello");
Check(await cache.GetAsync<string>("greeting") == "hello", "Caching.InMemory");

Check(await provider.GetRequiredService<IFeatureFlagService>().IsEnabledAsync("smoke"), "FeatureFlags.InMemory");

var hashing = provider.GetRequiredService<IHashingService>();
var hash = await hashing.HashAsync("secret");
Check(await hashing.VerifyAsync("secret", hash) && !await hashing.VerifyAsync("other", hash), "Security.Argon2");

var mfa = provider.GetRequiredService<IMfaProvider>();
var setup = await mfa.GenerateSetupAsync("smoke@example.test", "Kck");
Check(setup.ProvisioningUri.StartsWith("otpauth://", StringComparison.Ordinal) && !await mfa.ValidateCodeAsync(setup.SecretKey, "000000"), "Security.Totp");

await provider.GetRequiredService<IEventBus>().PublishAsync(new Ping());
Check(provider.GetRequiredService<PingHandler>().Received == 1, "EventBus.InMemory");

return failures.Count == 0 ? 0 : 1;

internal sealed record Count(int Value);

internal sealed record CountQuery(string Key) : IRequest<Count>, ICachableRequest
{
    public string CacheKey => Key;
    public bool BypassCache => false;
    public string? CacheGroupKey => null;
    public TimeSpan? SlidingExpiration => null;
}

internal sealed record Ping : IntegrationEvent;

internal sealed class PingHandler : IEventHandler<Ping>
{
    public int Received { get; private set; }

    public Task HandleAsync(Ping eventData, CancellationToken ct = default)
    {
        Received++;
        return Task.CompletedTask;
    }
}

internal static partial class SmokeLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "PASS {Check}")]
    public static partial void Passed(ILogger logger, string check);

    [LoggerMessage(Level = LogLevel.Error, Message = "FAIL {Check}")]
    public static partial void Failed(ILogger logger, string check);
}

[JsonSerializable(typeof(Count))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext;
