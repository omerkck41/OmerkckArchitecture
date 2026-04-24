using FluentAssertions;
using Kck.Security.Abstractions.Secrets;
using Kck.Security.Secrets.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Kck.Security.Secrets.AzureKeyVault.Tests;

public class AzureKeyVaultDiRegistrationTests
{
    [Fact]
    public void AddKckAzureKeyVault_ShouldRegisterSecretsManager()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckAzureKeyVault(o => o.VaultUri = "https://unit-test.vault.azure.net/");

        using var provider = services.BuildServiceProvider();
        var secrets = provider.GetRequiredService<ISecretsManager>();

        secrets.Should().BeOfType<AzureKeyVaultSecretsManager>();
    }

    [Fact]
    public void AddKckAzureKeyVault_ShouldRegisterAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckAzureKeyVault(o => o.VaultUri = "https://unit-test.vault.azure.net/");

        using var provider = services.BuildServiceProvider();
        var a = provider.GetRequiredService<ISecretsManager>();
        var b = provider.GetRequiredService<ISecretsManager>();

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void AddKckAzureKeyVault_CalledTwice_ShouldNotOverwrite()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckAzureKeyVault(o => o.VaultUri = "https://first.vault.azure.net/");
        services.AddKckAzureKeyVault(o => o.VaultUri = "https://second.vault.azure.net/");

        var count = services.Count(d => d.ServiceType == typeof(ISecretsManager));
        count.Should().Be(1);
    }

    [Fact]
    public void AddKckAzureKeyVault_WithSecretPrefix_ShouldConfigureOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKckAzureKeyVault(o =>
        {
            o.VaultUri = "https://unit-test.vault.azure.net/";
            o.SecretPrefix = "my-app";
        });

        using var provider = services.BuildServiceProvider();
        var secrets = provider.GetService<ISecretsManager>();

        secrets.Should().NotBeNull();
    }
}
