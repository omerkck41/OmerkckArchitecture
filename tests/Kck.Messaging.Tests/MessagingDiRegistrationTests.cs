using FluentAssertions;
using Kck.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Messaging.Tests;

public sealed class MessagingDiRegistrationTests
{
    [Fact]
    public void AddKckMessagingMailKit_RegistersIEmailProvider()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingMailKit(o =>
        {
            o.Host = "smtp.test.com";
            o.Port = 587;
        });

        var sp = services.BuildServiceProvider();
        var provider = sp.GetService<IEmailProvider>();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<Kck.Messaging.MailKit.MailKitEmailProvider>();
    }

    [Fact]
    public void AddKckMessagingSendGrid_RegistersIEmailProvider()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingSendGrid(o => o.ApiKey = "test-key");

        var sp = services.BuildServiceProvider();
        var provider = sp.GetService<IEmailProvider>();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<Kck.Messaging.SendGrid.SendGridEmailProvider>();
    }

    [Fact]
    public void AddKckMessagingAmazonSes_RegistersIEmailProvider()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingAmazonSes(o => o.Region = "eu-west-1");

        var sp = services.BuildServiceProvider();
        var provider = sp.GetService<IEmailProvider>();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<Kck.Messaging.AmazonSes.AmazonSesEmailProvider>();
    }
}
