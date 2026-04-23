using FluentAssertions;
using Kck.Messaging.MailKit;
using Xunit;

namespace Kck.Messaging.Tests;

public sealed class MailKitOptionsTests
{
    [Fact]
    public void DefaultPort_Is587()
    {
        var options = new MailKitOptions { Host = "smtp.test.com" };

        options.Port.Should().Be(587);
    }

    [Fact]
    public void DefaultUseSsl_IsTrue()
    {
        var options = new MailKitOptions { Host = "smtp.test.com" };

        options.UseSsl.Should().BeTrue();
    }
}
