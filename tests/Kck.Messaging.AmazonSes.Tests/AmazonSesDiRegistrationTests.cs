using FluentAssertions;
using Kck.Messaging.Abstractions;
using Kck.Messaging.AmazonSes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Messaging.AmazonSes.Tests;

public class AmazonSesDiRegistrationTests
{
    [Fact]
    public void AddKckMessagingAmazonSes_ShouldRegisterProvider()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingAmazonSes(o => o.Region = "eu-west-1");

        using var provider = services.BuildServiceProvider();
        var email = provider.GetRequiredService<IEmailProvider>();

        email.Should().BeOfType<AmazonSesEmailProvider>();
    }

    [Fact]
    public void AddKckMessagingAmazonSes_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingAmazonSes(o => o.Region = "us-east-1");

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IEmailProvider>();
        var second = provider.GetRequiredService<IEmailProvider>();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void AddKckMessagingAmazonSes_WithExplicitCredentials_ShouldResolveProvider()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingAmazonSes(o =>
        {
            o.Region = "eu-central-1";
            o.AccessKey = "AKIA-FAKE";
            o.SecretKey = "secret";
        });

        using var provider = services.BuildServiceProvider();
        var email = provider.GetService<IEmailProvider>();

        email.Should().NotBeNull();
    }
}
