using FluentAssertions;
using Kck.Security.Secrets.UserSecrets;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Kck.Security.Secrets.UserSecrets.Tests;

public class ConfigurationSecretsManagerTests
{
    private static ConfigurationSecretsManager CreateSut(params (string Key, string Value)[] pairs)
    {
        var dict = pairs.ToDictionary(p => p.Key, p => (string?)p.Value);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
        return new ConfigurationSecretsManager(config);
    }

    [Fact]
    public async Task GetSecretAsync_ExistingKey_ShouldReturnValue()
    {
        var sut = CreateSut(("Stripe:ApiKey", "sk_test_123"));

        var value = await sut.GetSecretAsync("Stripe:ApiKey");

        value.Should().Be("sk_test_123");
    }

    [Fact]
    public async Task GetSecretAsync_MissingKey_ShouldReturnNull()
    {
        var sut = CreateSut();

        var value = await sut.GetSecretAsync("NotThere");

        value.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task GetSecretAsync_EmptyKey_ShouldThrow(string key)
    {
        var sut = CreateSut();

        var act = async () => await sut.GetSecretAsync(key);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSecretAsync_Typed_ShouldDeserializeJson()
    {
        var sut = CreateSut(("Port", "8080"));

        var value = await sut.GetSecretAsync<int>("Port");

        value.Should().Be(8080);
    }

    [Fact]
    public async Task GetSecretAsync_Typed_MissingKey_ShouldReturnDefault()
    {
        var sut = CreateSut();

        var value = await sut.GetSecretAsync<int>("Missing");

        value.Should().Be(0);
    }

    [Fact]
    public async Task GetSecretAsync_Typed_ComplexObject_ShouldDeserialize()
    {
        var sut = CreateSut(("Config", "{\"Timeout\":30,\"Retries\":3}"));

        var value = await sut.GetSecretAsync<RetryConfig>("Config");

        value.Should().NotBeNull();
        value!.Timeout.Should().Be(30);
        value.Retries.Should().Be(3);
    }

    [Fact]
    public async Task SetSecretAsync_ShouldThrowNotSupported()
    {
        var sut = CreateSut();

        var act = async () => await sut.SetSecretAsync("key", "value");

        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ShouldReturnTrue()
    {
        var sut = CreateSut(("Present", "yes"));

        var exists = await sut.ExistsAsync("Present");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_MissingKey_ShouldReturnFalse()
    {
        var sut = CreateSut();

        var exists = await sut.ExistsAsync("Absent");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_EmptyKey_ShouldThrow()
    {
        var sut = CreateSut();

        var act = async () => await sut.ExistsAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class RetryConfig
    {
        public int Timeout { get; set; }
        public int Retries { get; set; }
    }
}
