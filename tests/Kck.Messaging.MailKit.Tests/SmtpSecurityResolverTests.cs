using AwesomeAssertions;
using Kck.Messaging.MailKit;
using MailKit.Security;
using Xunit;

namespace Kck.Messaging.MailKit.Tests;

public class SmtpSecurityResolverTests
{
    [Theory]
    [InlineData(465, true, SecureSocketOptions.SslOnConnect)]
    [InlineData(587, true, SecureSocketOptions.StartTls)]
    // UseSsl = true must never fall back to plaintext: mandatory STARTTLS on every non-465 port (security review 2026-11-01).
    [InlineData(25, true, SecureSocketOptions.StartTls)]
    [InlineData(2525, true, SecureSocketOptions.StartTls)]
    [InlineData(587, false, SecureSocketOptions.StartTlsWhenAvailable)]
    [InlineData(465, false, SecureSocketOptions.StartTlsWhenAvailable)]
    public void Resolve_WithoutExplicitOptions_DerivesFromPortAndUseSsl(
        int port, bool useSsl, SecureSocketOptions expected)
    {
        var options = new MailKitOptions { Host = "smtp.example.com", Port = port, UseSsl = useSsl };

        SmtpSecurityResolver.Resolve(options).Should().Be(expected);
    }

    [Fact]
    public void Resolve_WithExplicitOptions_ReturnsThem()
    {
        var implicitTlsOn587 = new MailKitOptions
        {
            Host = "smtp.example.com",
            Port = 587,
            UseSsl = false,
            SecureSocketOptions = SecureSocketOptions.SslOnConnect
        };
        var plain = new MailKitOptions
        {
            Host = "smtp.example.com",
            Port = 465,
            UseSsl = true,
            SecureSocketOptions = SecureSocketOptions.None
        };

        SmtpSecurityResolver.Resolve(implicitTlsOn587).Should().Be(SecureSocketOptions.SslOnConnect);
        SmtpSecurityResolver.Resolve(plain).Should().Be(SecureSocketOptions.None);
    }
}
