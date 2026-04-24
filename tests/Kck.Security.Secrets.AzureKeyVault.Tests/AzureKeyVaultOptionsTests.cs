using FluentAssertions;
using Kck.Security.Secrets.AzureKeyVault;
using Xunit;

namespace Kck.Security.Secrets.AzureKeyVault.Tests;

public class AzureKeyVaultOptionsTests
{
    [Fact]
    public void Defaults_SecretPrefix_ShouldBeNull()
    {
        var opts = new AzureKeyVaultOptions { VaultUri = "https://my-vault.vault.azure.net/" };

        opts.SecretPrefix.Should().BeNull();
    }

    [Fact]
    public void PrefixAndVaultUri_ShouldRoundTrip()
    {
        var opts = new AzureKeyVaultOptions
        {
            VaultUri = "https://prod.vault.azure.net/",
            SecretPrefix = "app1"
        };

        opts.VaultUri.Should().Be("https://prod.vault.azure.net/");
        opts.SecretPrefix.Should().Be("app1");
    }
}
