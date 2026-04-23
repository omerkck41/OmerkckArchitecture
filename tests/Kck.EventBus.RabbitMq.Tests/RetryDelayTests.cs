using FluentAssertions;
using Kck.EventBus.RabbitMq;
using Xunit;

namespace Kck.EventBus.RabbitMq.Tests;

public class RetryDelayTests
{
    [Theory]
    [InlineData(0, 2)]   // 2^1 = 2 sn
    [InlineData(1, 4)]   // 2^2 = 4 sn
    [InlineData(2, 8)]   // 2^3 = 8 sn
    [InlineData(3, 16)]  // 2^4 = 16 sn
    [InlineData(4, 30)]  // capped at 30 sn
    public void CalculateDelay_ShouldReturnExponentialWithCap(int attempt, int expectedSeconds)
    {
        var delay = RetryHelper.CalculateDelay(
            attempt,
            baseDelay: TimeSpan.FromSeconds(2),
            maxDelay: TimeSpan.FromSeconds(30));

        delay.TotalSeconds.Should().Be(expectedSeconds);
    }
}
