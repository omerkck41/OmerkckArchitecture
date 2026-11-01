using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AwesomeAssertions;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Kck.EventBus.AzureServiceBus.Tests;

/// <summary>
/// The event bus against a substituted <see cref="ServiceBusClient"/> (protected ctor, virtual members): what is sent,
/// which processors are created and what is disposed.
/// </summary>
public sealed class AzureServiceBusClientTests
{
    private readonly ServiceBusClient _client = Substitute.For<ServiceBusClient>();
    private readonly ServiceBusSender _sender = Substitute.For<ServiceBusSender>();
    private readonly ServiceBusProcessor _processor = Substitute.For<ServiceBusProcessor>();
    private readonly AzureServiceBusOptions _options = new() { TopicName = "orders", SubscriptionName = "billing" };

    public AzureServiceBusClientTests()
    {
        _client.CreateSender(Arg.Any<string>()).Returns(_sender);
        _client.CreateProcessor(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<ServiceBusProcessorOptions>())
            .Returns(_processor);
    }

    private AzureServiceBusEventBus CreateBus() => new(
        _options,
        new ServiceCollection().AddLogging().BuildServiceProvider(),
        NullLogger<AzureServiceBusEventBus>.Instance,
        _client);

    [Fact]
    public async Task PublishAsync_SendsJsonBodyWithSubjectToTopicSender()
    {
        ServiceBusMessage? sent = null;
        await _sender.SendMessageAsync(Arg.Do<ServiceBusMessage>(m => sent = m), Arg.Any<CancellationToken>());
        await using var bus = CreateBus();
        var payment = new TestPaymentProcessedEvent("pay-1", 12.5m);

        await bus.PublishAsync(payment);

        _client.Received(1).CreateSender("orders");
        sent.Should().NotBeNull();
        sent!.Subject.Should().Be(nameof(TestPaymentProcessedEvent));
        sent.MessageId.Should().Be(payment.Id.ToString());
        sent.ContentType.Should().Be("application/json");
        sent.ApplicationProperties["EventType"].Should().Be(nameof(TestPaymentProcessedEvent));
        using var body = JsonDocument.Parse(sent.Body.ToMemory());
        body.RootElement.GetProperty("paymentId").GetString().Should().Be("pay-1");
        body.RootElement.GetProperty("amount").GetDecimal().Should().Be(12.5m);
    }

    [Fact]
    public async Task PublishAsync_Concurrent_CreatesSenderOnce()
    {
        await using var bus = CreateBus();

        await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(i => bus.PublishAsync(new TestPaymentProcessedEvent($"pay-{i}", i))));

        _client.Received(1).CreateSender(Arg.Any<string>());
        await _sender.Received(20).SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Subscribe_CreatesProcessorForSubscription()
    {
        // Subscribe records the handler; the consumer hosted service creates one processor per event type on start.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_client);
        services.AddKckEventBus(bus => bus.UseAzureServiceBus(o =>
        {
            o.TopicName = "orders";
            o.SubscriptionName = "billing";
        }));
        await using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IEventBus>()
            .Subscribe<TestPaymentProcessedEvent, TestPaymentProcessedHandler>();
        var consumer = provider.GetServices<IHostedService>().Should().ContainSingle().Subject;
        // BackgroundService runs ExecuteAsync in the background: wait until the processor was started.
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _processor.StartProcessingAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            started.TrySetResult();
            return Task.CompletedTask;
        });

        await consumer.StartAsync(CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await consumer.StopAsync(CancellationToken.None);

        _client.Received(1).CreateProcessor(
            "orders",
            $"billing.{nameof(TestPaymentProcessedEvent)}",
            Arg.Is<ServiceBusProcessorOptions>(o => !o.AutoCompleteMessages && o.MaxConcurrentCalls == 10));
        await _processor.Received(1).StartProcessingAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void UseAzureServiceBus_OnlyKeyedClientRegistered_StillRequiresConnectionString()
    {
        // The bus resolves the unkeyed ServiceBusClient; a keyed one (for another namespace) must not satisfy the check.
        var services = new ServiceCollection();
        services.AddKeyedSingleton("other-namespace", _client);

        var register = () => services.AddKckEventBus(bus => bus.UseAzureServiceBus(_ => { }));

        register.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("ConnectionString");
    }

    [Fact]
    public async Task DisposeAsync_DisposesProcessorsSenderAndClient()
    {
        // A bus that owns its client (the connection-string path) disposes all three.
        var bus = new AzureServiceBusEventBus(
            _options,
            new ServiceCollection().AddLogging().BuildServiceProvider(),
            NullLogger<AzureServiceBusEventBus>.Instance,
            _client,
            ownsClient: true);
        bus.Subscribe<TestPaymentProcessedEvent, TestPaymentProcessedHandler>();
        await bus.StartProcessingAsync();

        await bus.DisposeAsync();

        await _processor.Received(1).StopProcessingAsync(Arg.Any<CancellationToken>());
        await _processor.Received(1).DisposeAsync();
        await _sender.Received(1).DisposeAsync();
        await _client.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WithInjectedClient_LeavesTheClientOpen()
    {
        // A client handed in (or registered in the container) belongs to its owner, like the shared Redis connection.
        var bus = CreateBus();
        await bus.PublishAsync(new TestPaymentProcessedEvent("pay-1", 1m));

        await bus.DisposeAsync();

        await _sender.Received(1).DisposeAsync();
        await _client.DidNotReceive().DisposeAsync();
    }
}
