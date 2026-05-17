using FluentAssertions;
using Kck.EventBus.AzureServiceBus.DependencyInjection;
using Xunit;

namespace Kck.EventBus.AzureServiceBus.Tests;

public sealed class AzureServiceBusOptionsValidatorTests
{
    private readonly AzureServiceBusOptionsValidator _sut = new();

    [Fact]
    public void Validate_AllRequired_ReturnsSuccess()
    {
        var opts = new AzureServiceBusOptions
        {
            ConnectionString = "Endpoint=sb://ns.servicebus.windows.net/;SharedAccessKeyName=k;SharedAccessKey=v",
            TopicName = "events",
            SubscriptionName = "my-service"
        };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyConnectionString_ReturnsFail(string cs)
    {
        var opts = new AzureServiceBusOptions { ConnectionString = cs, TopicName = "t", SubscriptionName = "s" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString");
    }

    [Fact]
    public void Validate_EmptyTopicName_ReturnsFail()
    {
        var opts = new AzureServiceBusOptions { ConnectionString = "Endpoint=sb://x", TopicName = "", SubscriptionName = "s" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("TopicName");
    }

    [Fact]
    public void Validate_EmptySubscriptionName_ReturnsFail()
    {
        var opts = new AzureServiceBusOptions { ConnectionString = "Endpoint=sb://x", TopicName = "t", SubscriptionName = "" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("SubscriptionName");
    }
}
