using FluentAssertions;
using Kck.Security.Abstractions.Secrets;
using Kck.Security.Secrets.UserSecrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.Security.Secrets.UserSecrets.Tests;

public class UserSecretsDiRegistrationTests
{
    [Fact]
    public void AddKckUserSecrets_ShouldRegisterConfigurationSecretsManager()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddKckUserSecrets();

        using var provider = services.BuildServiceProvider();
        var secrets = provider.GetRequiredService<ISecretsManager>();

        secrets.Should().BeOfType<ConfigurationSecretsManager>();
    }

    [Fact]
    public void AddKckUserSecrets_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddKckUserSecrets();

        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<ISecretsManager>();
        var second = provider.GetRequiredService<ISecretsManager>();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void AddKckUserSecrets_CalledTwice_ShouldNotOverwrite()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddKckUserSecrets();
        services.AddKckUserSecrets();

        var descriptors = services.Where(d => d.ServiceType == typeof(ISecretsManager)).ToList();
        descriptors.Should().HaveCount(1);
    }
}
