using FluentAssertions;
using Kck.Messaging.MailKit;
using Xunit;

namespace Kck.Messaging.MailKit.Tests;

public class MailKitOptionsTests
{
    [Fact]
    public void DefaultPort_ShouldBe587()
    {
        var opts = new MailKitOptions { Host = "smtp.example.com" };

        opts.Port.Should().Be(587);
    }

    [Fact]
    public void DefaultUseSsl_ShouldBeTrue()
    {
        var opts = new MailKitOptions { Host = "smtp.example.com" };

        opts.UseSsl.Should().BeTrue();
    }

    [Fact]
    public void DefaultPoolSize_ShouldBeFive()
    {
        var opts = new MailKitOptions { Host = "smtp.example.com" };

        opts.PoolSize.Should().Be(5);
    }

    [Fact]
    public void HostIsRequired_ShouldCompileWithRequiredInitializer()
    {
        var opts = new MailKitOptions
        {
            Host = "smtp.gmail.com",
            Port = 465,
            UseSsl = true,
            UserName = "user@example.com",
            Password = "pw",
            PoolSize = 10
        };

        opts.Host.Should().Be("smtp.gmail.com");
        opts.Port.Should().Be(465);
        opts.PoolSize.Should().Be(10);
    }
}
