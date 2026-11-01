using AwesomeAssertions;
using Kck.EventBus.RabbitMq;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

public class RabbitMqOptionsTests
{
    [Fact]
    public void Defaults_ShouldHaveExpectedValues()
    {
        var options = new RabbitMqOptions();

        options.HostName.Should().BeEmpty();
        options.Port.Should().Be(5672);
        options.UserName.Should().BeEmpty();
        options.Password.Should().BeEmpty();
        options.VirtualHost.Should().Be("/");
        options.ExchangeName.Should().Be("kck.eventbus");
        options.ExchangeType.Should().Be("topic");
        options.QueuePrefix.Should().Be("kck");
        options.RetryCount.Should().Be(5);
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(2));
        options.MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(30));
        options.MaxDeliveryCount.Should().Be(5);
        options.PrefetchCount.Should().Be(10);
        options.DeadLetterEnabled.Should().BeTrue();
        options.DeadLetterExchangeName.Should().BeNull();
        options.DeadLetterQueueName.Should().BeNull();
    }

    [Fact]
    public void EffectiveDeadLetterNames_WhenNull_DeriveFromQueuePrefix()
    {
        var options = new RabbitMqOptions { ExchangeName = "x", QueuePrefix = "p" };

        // Per service: services sharing ExchangeName must not share the fanout dead-letter exchange.
        options.EffectiveDeadLetterExchangeName.Should().Be("p.dlx");
        options.EffectiveDeadLetterQueueName.Should().Be("p.dlq");
    }

    [Fact]
    public void EffectiveDeadLetterNames_WhenSet_AreUsed()
    {
        var options = new RabbitMqOptions
        {
            ExchangeName = "x",
            QueuePrefix = "p",
            DeadLetterExchangeName = "custom.dlx",
            DeadLetterQueueName = "custom.dlq"
        };

        options.EffectiveDeadLetterExchangeName.Should().Be("custom.dlx");
        options.EffectiveDeadLetterQueueName.Should().Be("custom.dlq");
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var options = new RabbitMqOptions
        {
            HostName = "rabbitmq.prod",
            Port = 5673,
            UserName = "admin",
            Password = "secret",
            VirtualHost = "/prod",
            ExchangeName = "my.exchange",
            ExchangeType = "fanout",
            QueuePrefix = "myapp",
            RetryCount = 10,
            RetryDelay = TimeSpan.FromSeconds(5),
            MaxRetryDelay = TimeSpan.FromSeconds(60),
            MaxDeliveryCount = 10
        };

        options.HostName.Should().Be("rabbitmq.prod");
        options.Port.Should().Be(5673);
        options.UserName.Should().Be("admin");
        options.Password.Should().Be("secret");
        options.VirtualHost.Should().Be("/prod");
        options.ExchangeName.Should().Be("my.exchange");
        options.ExchangeType.Should().Be("fanout");
        options.QueuePrefix.Should().Be("myapp");
        options.RetryCount.Should().Be(10);
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(5));
        options.MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(60));
        options.MaxDeliveryCount.Should().Be(10);
    }
}
