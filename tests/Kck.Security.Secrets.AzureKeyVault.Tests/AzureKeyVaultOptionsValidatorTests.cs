using FluentAssertions;
using Kck.Security.Secrets.AzureKeyVault.DependencyInjection;
using Xunit;

namespace Kck.Security.Secrets.AzureKeyVault.Tests;

public sealed class AzureKeyVaultOptionsValidatorTests
{
    private readonly AzureKeyVaultOptionsValidator _sut = new();

    [Fact]
    public void Validate_ValidHttpsUri_ReturnsSuccess()
    {
        var opts = new AzureKeyVaultOptions { VaultUri = "https://myvault.vault.azure.net/" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyVaultUri_ReturnsFail(string uri)
    {
        var opts = new AzureKeyVaultOptions { VaultUri = uri };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("VaultUri");
    }

    [Theory]
    [InlineData("http://myvault.vault.azure.net/")]
    [InlineData("not-a-url")]
    public void Validate_NonHttpsUri_ReturnsFail(string uri)
    {
        var opts = new AzureKeyVaultOptions { VaultUri = uri };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("VaultUri");
    }
}
