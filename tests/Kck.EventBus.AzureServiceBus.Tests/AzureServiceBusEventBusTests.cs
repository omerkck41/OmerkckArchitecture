using FluentAssertions;
using Kck.EventBus.Abstractions;
using Kck.EventBus.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.EventBus.AzureServiceBus.Tests;

public sealed record TestPaymentProcessedEvent(string PaymentId, decimal Amount) : IntegrationEvent;

public sealed class TestPaymentProcessedHandler : IEventHandler<TestPaymentProcessedEvent>
{
    public List<TestPaymentProcessedEvent> HandledEvents { get; } = [];

    public Task HandleAsync(TestPaymentProcessedEvent @event, CancellationToken ct = default)
    {
        HandledEvents.Add(@event);
        return Task.CompletedTask;
    }
}

public class AzureServiceBusEventBusTests
{
    [Fact]
    public void Subscribe_ShouldNotThrow()
    {
        var bus = CreateBus();

        var act = () => bus.Subscribe<TestPaymentProcessedEvent, TestPaymentProcessedHandler>();

        act.Should().NotThrow();
    }

    [Fact]
    public void UseAzureServiceBus_ShouldRegisterEventBus()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckEventBus(bus =>
        {
            bus.UseAzureServiceBus(asb =>
            {
                asb.ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=val";
                asb.TopicName = "my-topic";
            });
        });

        var provider = services.BuildServiceProvider();
        var eventBus = provider.GetService<IEventBus>();

        eventBus.Should().NotBeNull();
        eventBus.Should().BeOfType<AzureServiceBusEventBus>();
    }

    [Fact]
    public void UseAzureServiceBus_WithEmptyConnectionString_ShouldThrow()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKckEventBus(bus =>
        {
            bus.UseAzureServiceBus(_ => { });
        });

        act.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("ConnectionString");
    }

    private static AzureServiceBusEventBus CreateBus()
    {
        var options = new AzureServiceBusOptions
        {
            ConnectionString = "Endpoint=sb://invalid-for-test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA=="
        };
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var logger = new NullLogger<AzureServiceBusEventBus>();
        return new AzureServiceBusEventBus(options, serviceProvider, logger);
    }
}
