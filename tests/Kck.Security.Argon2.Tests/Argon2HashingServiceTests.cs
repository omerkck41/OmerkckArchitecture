using FluentAssertions;
using Kck.Security.Argon2;
using Kck.Testing;
using Xunit;

namespace Kck.Security.Argon2.Tests;

public class Argon2HashingServiceTests
{
    private readonly Argon2HashingService _sut;

    public Argon2HashingServiceTests()
    {
        var options = new StaticOptionsMonitor<Argon2Options>(new Argon2Options());
        _sut = new Argon2HashingService(options);
    }

    [Fact]
    public async Task HashAsync_ShouldReturnArgon2idFormattedHash()
    {
        var hash = await _sut.HashAsync("MyPassword123!");
        hash.Should().StartWith("$argon2id$v=19$");
    }

    [Fact]
    public async Task VerifyAsync_CorrectPassword_ShouldReturnTrue()
    {
        var hash = await _sut.HashAsync("MyPassword123!");
        var result = await _sut.VerifyAsync("MyPassword123!", hash);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyAsync_WrongPassword_ShouldReturnFalse()
    {
        var hash = await _sut.HashAsync("MyPassword123!");
        var result = await _sut.VerifyAsync("WrongPassword", hash);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HashAsync_SamePassword_ShouldProduceDifferentHashes()
    {
        var hash1 = await _sut.HashAsync("MyPassword123!");
        var hash2 = await _sut.HashAsync("MyPassword123!");
        hash1.Should().NotBe(hash2, "her hash farkli salt kullanmali");
    }

    [Fact]
    public async Task HashAsync_EmptyPassword_ShouldThrowArgumentException()
    {
        // Konscious.Security.Cryptography.Argon2 requires a non-empty password byte array
        await _sut.Invoking(s => s.HashAsync(""))
            .Should().ThrowAsync<ArgumentException>();
    }
}
