using FluentAssertions;
using Kck.Messaging.AmazonSes;
using Xunit;

namespace Kck.Messaging.AmazonSes.Tests;

public class AmazonSesOptionsTests
{
    [Fact]
    public void Defaults_OptionalCredentials_ShouldBeNull()
    {
        var opts = new AmazonSesOptions { Region = "eu-west-1" };

        opts.AccessKey.Should().BeNull();
        opts.SecretKey.Should().BeNull();
    }

    [Fact]
    public void AllFields_ShouldRoundTrip()
    {
        var opts = new AmazonSesOptions
        {
            Region = "us-east-1",
            AccessKey = "AKIA-FAKE",
            SecretKey = "secret-fake"
        };

        opts.Region.Should().Be("us-east-1");
        opts.AccessKey.Should().Be("AKIA-FAKE");
        opts.SecretKey.Should().Be("secret-fake");
    }
}
