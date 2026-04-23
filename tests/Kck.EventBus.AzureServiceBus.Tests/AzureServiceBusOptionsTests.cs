using FluentAssertions;
using Kck.EventBus.AzureServiceBus;
using Xunit;

namespace Kck.EventBus.AzureServiceBus.Tests;

public class AzureServiceBusOptionsTests
{
    [Fact]
    public void Defaults_ShouldHaveExpectedValues()
    {
        var options = new AzureServiceBusOptions();

        options.ConnectionString.Should().BeEmpty();
        options.TopicName.Should().Be("kck-eventbus");
        options.SubscriptionName.Should().Be("default");
        options.MaxConcurrentCalls.Should().Be(10);
        options.MaxAutoLockRenewalDuration.Should().Be(TimeSpan.FromMinutes(5));
        options.RetryCount.Should().Be(3);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var options = new AzureServiceBusOptions
        {
            ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=val",
            TopicName = "my-topic",
            SubscriptionName = "my-sub",
            MaxConcurrentCalls = 20,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
            RetryCount = 5
        };

        options.ConnectionString.Should().Contain("test.servicebus.windows.net");
        options.TopicName.Should().Be("my-topic");
        options.SubscriptionName.Should().Be("my-sub");
        options.MaxConcurrentCalls.Should().Be(20);
        options.MaxAutoLockRenewalDuration.Should().Be(TimeSpan.FromMinutes(10));
        options.RetryCount.Should().Be(5);
    }
}
