using AwesomeAssertions;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.EventBus.InMemory.Tests;

/// <summary>
/// v4 (ADR-0028 K4): the legacy AddKckEventBusInMemory registration methods are removed;
/// AddKckEventBus(b => b.UseInMemory()) is the only registration path.
/// </summary>
public sealed class RemovedApiTests
{
    [Fact]
    public void AddKckEventBusInMemory_IsRemoved()
    {
        typeof(KckEventBusInMemoryServiceCollectionExtensions).GetMethods()
            .Where(m => m.Name == "AddKckEventBusInMemory")
            .Should().BeEmpty();
    }

    [Fact]
    public void AddKckEventBus_UseInMemory_RegistersEventBus()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckEventBus(b => b.UseInMemory());

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IEventBus>().Should().BeOfType<InMemoryEventBus>();
    }
}
