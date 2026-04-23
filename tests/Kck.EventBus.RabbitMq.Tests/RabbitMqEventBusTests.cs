using FluentAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

public sealed record TestOrderCreatedEvent(string OrderId) : IntegrationEvent;

public sealed class TestOrderCreatedHandler : IEventHandler<TestOrderCreatedEvent>
{
    public List<TestOrderCreatedEvent> HandledEvents { get; } = [];

    public Task HandleAsync(TestOrderCreatedEvent @event, CancellationToken ct = default)
    {
        HandledEvents.Add(@event);
        return Task.CompletedTask;
    }
}

public class RabbitMqEventBusTests
{
    [Fact]
    public void Subscribe_ShouldNotThrow()
    {
        var bus = CreateBus();

        var act = () => bus.Subscribe<TestOrderCreatedEvent, TestOrderCreatedHandler>();

        act.Should().NotThrow();
    }

    [Fact]
    public void UseRabbitMq_ShouldRegisterEventBus()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckEventBus(bus =>
        {
            bus.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = "test-host";
                rabbit.Port = 5673;
                rabbit.UserName = "user";
                rabbit.Password = "pass";
            });
        });

        var provider = services.BuildServiceProvider();
        var eventBus = provider.GetService<IEventBus>();

        eventBus.Should().NotBeNull();
        eventBus.Should().BeOfType<RabbitMqEventBus>();
    }

    [Fact]
    public void UseRabbitMq_WithEmptyHostName_ShouldThrow()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckEventBus(bus =>
        {
            bus.UseRabbitMq(_ => { });
        });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("HostName");
    }

    private static RabbitMqEventBus CreateBus()
    {
        var options = new RabbitMqOptions { HostName = "invalid-host-for-test" };
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var logger = new NullLogger<RabbitMqEventBus>();
        return new RabbitMqEventBus(options, serviceProvider, logger);
    }
}
