using FluentAssertions;
using Kck.Messaging.Abstractions;
using Kck.Messaging.SendGrid;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Messaging.SendGrid.Tests;

public class SendGridDiRegistrationTests
{
    [Fact]
    public void AddKckMessagingSendGrid_ShouldRegisterProvider()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingSendGrid(o => o.ApiKey = "SG.fake");

        using var provider = services.BuildServiceProvider();
        var email = provider.GetRequiredService<IEmailProvider>();

        email.Should().BeOfType<SendGridEmailProvider>();
    }

    [Fact]
    public void AddKckMessagingSendGrid_ShouldResolveAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingSendGrid(o => o.ApiKey = "SG.x");

        using var provider = services.BuildServiceProvider();
        var a = provider.GetRequiredService<IEmailProvider>();
        var b = provider.GetRequiredService<IEmailProvider>();

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void AddKckMessagingSendGrid_TwiceRegistered_ShouldNotOverwrite()
    {
        var services = new ServiceCollection();

        services.AddKckMessagingSendGrid(o => o.ApiKey = "one");
        services.AddKckMessagingSendGrid(o => o.ApiKey = "two");

        var descriptorCount = services.Count(d => d.ServiceType == typeof(IEmailProvider));
        descriptorCount.Should().Be(1);
    }
}
