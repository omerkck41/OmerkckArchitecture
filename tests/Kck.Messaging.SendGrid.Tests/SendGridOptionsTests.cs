using FluentAssertions;
using Kck.Messaging.SendGrid;
using Xunit;

namespace Kck.Messaging.SendGrid.Tests;

public class SendGridOptionsTests
{
    [Fact]
    public void ApiKey_ShouldRoundTrip()
    {
        var opts = new SendGridOptions { ApiKey = "SG.fake.key" };

        opts.ApiKey.Should().Be("SG.fake.key");
    }
}
