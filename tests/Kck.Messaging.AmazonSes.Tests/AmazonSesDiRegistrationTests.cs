using AwesomeAssertions;
using Kck.Messaging.Abstractions;
using Kck.Messaging.AmazonSes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    [Fact]
    public async Task AddKckMessagingAmazonSes_WithRegisteredSesClient_SendsThroughIt()
    {
        var (client, fake) = FakeSesClient.Create();
        var services = new ServiceCollection();
        services.AddSingleton(client);

        services.AddKckMessagingAmazonSes(o => o.Region = "eu-west-1");

        using var provider = services.BuildServiceProvider();
        var email = provider.GetRequiredService<IEmailProvider>();
        await email.SendAsync(new EmailMessage
        {
            From = "shop@example.com",
            Subject = "Hi",
            Body = "Hello",
            Recipients = [new EmailRecipient("ada@example.com")]
        });

        email.Should().BeOfType<AmazonSesEmailProvider>();
        fake.Requests.Should().ContainSingle();
    }

    [Fact]
    public void AddKckMessagingAmazonSes_WithRegisteredSesClientAndEmptyOptions_PassesValidation()
    {
        var (client, _) = FakeSesClient.Create();
        var services = new ServiceCollection();
        services.AddSingleton(client);
        services.AddKckMessagingAmazonSes(_ => { });
        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptionsMonitor<AmazonSesOptions>>().CurrentValue;

        act.Should().NotThrow();
        provider.GetRequiredService<IEmailProvider>().Should().BeOfType<AmazonSesEmailProvider>();
    }

    [Fact]
    public void AddKckMessagingAmazonSes_WithoutClientAndEmptyOptions_FailsValidation()
    {
        var services = new ServiceCollection();
        services.AddKckMessagingAmazonSes(_ => { });
        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptionsMonitor<AmazonSesOptions>>().CurrentValue;

        act.Should().Throw<OptionsValidationException>();
    }
}
