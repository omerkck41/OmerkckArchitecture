using AwesomeAssertions;
using Kck.EventBus.RabbitMq;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

public class DeliveryAttemptResolutionTests
{
    [Fact]
    public void ResolveDeliveryAttempt_WithMessageId_CountsDeliveriesOfThatMessage()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());

        var first = RabbitMqEventBus.ResolveDeliveryAttempt("m1", redelivered: false, maxDeliveryCount: 5, tracker);
        var second = RabbitMqEventBus.ResolveDeliveryAttempt("m1", redelivered: true, maxDeliveryCount: 5, tracker);

        first.Should().Be(1);
        second.Should().Be(2, "with a MessageId the in-process count decides, not the Redelivered flag");
        tracker.Count.Should().Be(1);
    }

    [Fact]
    public void ResolveDeliveryAttempt_WithoutMessageId_FirstDelivery_ReturnsOne()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());

        RabbitMqEventBus.ResolveDeliveryAttempt(null, redelivered: false, maxDeliveryCount: 5, tracker).Should().Be(1);
        tracker.Count.Should().Be(0);
    }

    [Fact]
    public void ResolveDeliveryAttempt_WithoutMessageId_Redelivered_ReturnsMaxDeliveryCount()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());

        RabbitMqEventBus.ResolveDeliveryAttempt(null, redelivered: true, maxDeliveryCount: 5, tracker).Should().Be(5);
        tracker.Count.Should().Be(0);
    }

    [Fact]
    public void ResolveDeliveryAttempt_WithEmptyMessageId_IsTreatedAsMissing()
    {
        var tracker = new DeliveryAttemptTracker(new TestTimeProvider());

        RabbitMqEventBus.ResolveDeliveryAttempt(string.Empty, redelivered: true, maxDeliveryCount: 3, tracker).Should().Be(3);
        tracker.Count.Should().Be(0);
    }
}
