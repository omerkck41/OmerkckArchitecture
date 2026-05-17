using FluentAssertions;
using Kck.FileStorage.FluentFtp.DependencyInjection;
using Xunit;

namespace Kck.FileStorage.FluentFtp.Tests;

public sealed class FluentFtpOptionsValidatorTests
{
    private readonly FluentFtpOptionsValidator _sut = new();

    [Fact]
    public void Validate_ValidOptions_ReturnsSuccess()
    {
        var opts = new FluentFtpOptions { Host = "ftp.example.com", Port = 21 };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyHost_ReturnsFail(string host)
    {
        var opts = new FluentFtpOptions { Host = host, Port = 21 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Host");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void Validate_InvalidPort_ReturnsFail(int port)
    {
        var opts = new FluentFtpOptions { Host = "ftp.example.com", Port = port };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Port");
    }
}
