using AwesomeAssertions;
using Kck.Messaging.MailKit.DependencyInjection;
using MailKit.Security;
using Xunit;

namespace Kck.Messaging.MailKit.Tests;

public sealed class MailKitOptionsValidatorTests
{
    private readonly MailKitOptionsValidator _sut = new();

    [Fact]
    public void Validate_ValidOptions_ReturnsSuccess()
    {
        var opts = new MailKitOptions { Host = "smtp.example.com", Port = 587 };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyHost_ReturnsFail(string host)
    {
        var opts = new MailKitOptions { Host = host, Port = 587 };
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
        var opts = new MailKitOptions { Host = "smtp.example.com", Port = port };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Port");
    }

    [Fact]
    public void Validate_BothInvalid_FailureMessageContainsBothFields()
    {
        var opts = new MailKitOptions { Host = "", Port = 0 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Host").And.Contain("Port");
    }

    [Fact]
    public void Validate_UserNameWithSecureSocketOptionsNone_ReturnsFail()
    {
        var opts = new MailKitOptions
        {
            Host = "smtp.example.com",
            Port = 25,
            SecureSocketOptions = SecureSocketOptions.None,
            UserName = "mailer"
        };

        var result = _sut.Validate(null, opts);

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("UserName").And.Contain("plaintext");
    }

    [Fact]
    public void Validate_NoUserNameWithSecureSocketOptionsNone_ReturnsSuccess()
    {
        // An unauthenticated relay on a trusted network sends no credentials.
        var opts = new MailKitOptions { Host = "relay.internal", Port = 25, SecureSocketOptions = SecureSocketOptions.None };

        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(SecureSocketOptions.StartTls)]
    [InlineData(SecureSocketOptions.SslOnConnect)]
    public void Validate_UserNameWithTls_ReturnsSuccess(SecureSocketOptions mode)
    {
        var opts = new MailKitOptions
        {
            Host = "smtp.example.com",
            Port = 587,
            SecureSocketOptions = mode,
            UserName = "mailer"
        };

        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }
}
