using AwesomeAssertions;
using Kck.BackgroundJobs.Abstractions;
using Kck.BackgroundJobs.Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Bundle.WorkerService.Tests;

/// <summary>
/// v4 (ADR-0028 K5, D3 + D8a): Quartz is the only scheduler in the bundle; a Hangfire configuration fails fast instead
/// of silently switching schedulers, and the bundle no longer references Hangfire (LGPL-3.0).
/// </summary>
public sealed class WorkerServiceDefaultsTests
{
    private static ServiceCollection Register(params (string Key, string Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)))
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckWorkerServiceDefaults(configuration);
        return services;
    }

    [Fact]
    public void ObservabilitySection_IsIgnored_AndThePropertyIsRemoved()
    {
        // The bundle never applied Kck:Observability; the KCK0002 property was removed in 5.0. A leftover section is ignored.
        using var provider = Register(("Kck:Observability:Tracing", "false"), ("Kck:Observability:OtlpEndpoint", "http://x:4317"))
            .BuildServiceProvider();

        typeof(KckWorkerServiceDefaults).GetProperty("Observability").Should().BeNull();
        provider.GetService<IJobScheduler>().Should().NotBeNull();
    }

    [Fact]
    public void WithoutConfiguration_UsesQuartz()
    {
        using var provider = Register().BuildServiceProvider();

        provider.GetRequiredService<IJobScheduler>().Should().BeOfType<QuartzJobScheduler>();
    }

    [Fact]
    public void QuartzProvider_UsesQuartz()
    {
        using var provider = Register(("Kck:BackgroundJobs:Provider", "quartz")).BuildServiceProvider();

        provider.GetRequiredService<IJobScheduler>().Should().BeOfType<QuartzJobScheduler>();
    }

    [Fact]
    public void HangfireProvider_FailsWithMigrationGuidance()
    {
        var act = () => Register(("Kck:BackgroundJobs:Provider", "Hangfire"));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Kck.BackgroundJobs.Hangfire*")
            .And.Message.Should().Contain("AddKckBackgroundJobsHangfire");
    }

    [Fact]
    public void UnknownProvider_FailsInsteadOfFallingBack()
    {
        var act = () => Register(("Kck:BackgroundJobs:Provider", "Coravel"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*Coravel*");
    }

    [Fact]
    public void Bundle_DoesNotReferenceHangfire()
    {
        typeof(Microsoft.Extensions.DependencyInjection.KckBundleWorkerServiceCollectionExtensions).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .Should().NotContain(name => name!.StartsWith("Hangfire", StringComparison.Ordinal)
                                         || name == "Kck.BackgroundJobs.Hangfire");
    }
}
