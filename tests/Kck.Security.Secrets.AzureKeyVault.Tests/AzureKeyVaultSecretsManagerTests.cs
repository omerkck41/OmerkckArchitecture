using Azure;
using Azure.Security.KeyVault.Secrets;
using AwesomeAssertions;
using Kck.Security.Abstractions.Secrets;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Kck.Security.Secrets.AzureKeyVault.Tests;

/// <summary>The secrets manager against a substituted <see cref="SecretClient"/> (protected ctor, virtual members).</summary>
public sealed class AzureKeyVaultSecretsManagerTests
{
    private readonly SecretClient _client = Substitute.For<SecretClient>();

    private AzureKeyVaultSecretsManager Create(string? prefix = "app") => new(
        new StaticOptionsMonitor<AzureKeyVaultOptions>(new AzureKeyVaultOptions { VaultUri = "", SecretPrefix = prefix }),
        NullLogger<AzureKeyVaultSecretsManager>.Instance,
        _client);

    // SecretClient 4.12 has two GetSecretAsync overloads (with and without SecretContentType?); both are stubbed so
    // the test does not depend on which one the provider's call binds to.
    private void Returns(string name, string value)
    {
        var response = Response.FromValue(
            SecretModelFactory.KeyVaultSecret(SecretModelFactory.SecretProperties(name: name), value),
            Substitute.For<Response>());
        _client.GetSecretAsync(name, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(response);
        _client.GetSecretAsync(name, Arg.Any<string?>(), Arg.Any<SecretContentType?>(), Arg.Any<CancellationToken>())
            .Returns(response);
    }

    private void NotFound(string name)
    {
        var notFound = new RequestFailedException(404, "SecretNotFound");
        _client.GetSecretAsync(name, Arg.Any<string?>(), Arg.Any<CancellationToken>()).ThrowsAsync(notFound);
        _client.GetSecretAsync(name, Arg.Any<string?>(), Arg.Any<SecretContentType?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(notFound);
    }

    private IEnumerable<string?> RequestedSecretNames() => _client.ReceivedCalls()
        .Where(c => c.GetMethodInfo().Name == nameof(SecretClient.GetSecretAsync))
        .Select(c => c.GetArguments()[0] as string);

    [Fact]
    public async Task GetSecretAsync_WithPrefix_RequestsPrefixedName()
    {
        Returns("app-db-password", "s3cret");

        var value = await Create("app").GetSecretAsync("db-password");

        value.Should().Be("s3cret");
        RequestedSecretNames().Should().Equal("app-db-password");
    }

    [Fact]
    public async Task GetSecretAsync_NotFound404_ReturnsNull()
    {
        NotFound("app-missing");

        var value = await Create().GetSecretAsync("missing");

        value.Should().BeNull();
    }

    [Fact]
    public async Task GetSecretAsyncT_DeserializesJson()
    {
        Returns("app-smtp", """{"Host":"smtp.example.com","Port":587}""");

        var value = await Create().GetSecretAsync<SmtpSecret>("smtp");

        value.Should().BeEquivalentTo(new SmtpSecret { Host = "smtp.example.com", Port = 587 });
    }

    [Fact]
    public async Task SetSecretAsync_SendsPrefixedNameAndValue()
    {
        _client.SetSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(new KeyVaultSecret("app-api-key", "v1"), Substitute.For<Response>()));

        await Create().SetSecretAsync("api-key", "v1");

        await _client.Received(1).SetSecretAsync("app-api-key", "v1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistsAsync_404_ReturnsFalse()
    {
        NotFound("app-missing");

        var exists = await Create().ExistsAsync("missing");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task AddKckAzureKeyVault_WithRegisteredClient_UsesIt()
    {
        Returns("from-registered-client", "ok");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_client);
        services.AddKckAzureKeyVault(_ => { });

        await using var provider = services.BuildServiceProvider(validateScopes: true);
        var secrets = provider.GetRequiredService<ISecretsManager>();

        // No VaultUri: the validator accepts it because a SecretClient is registered.
        var options = () => provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureKeyVaultOptions>>().Value;
        options.Should().NotThrow();
        (await secrets.GetSecretAsync("from-registered-client")).Should().Be("ok");
    }

    public sealed class SmtpSecret
    {
        public string? Host { get; init; }
        public int Port { get; init; }
    }
}
