using MailKit.Security;

namespace Kck.Messaging.MailKit;

/// <summary>
/// Picks the <see cref="SecureSocketOptions"/> used to connect to the SMTP server.
/// </summary>
internal static class SmtpSecurityResolver
{
    /// <summary>
    /// Returns <see cref="MailKitOptions.SecureSocketOptions"/> when it is set; otherwise derives the mode from
    /// <see cref="MailKitOptions.UseSsl"/> and <see cref="MailKitOptions.Port"/>: <c>UseSsl = true</c> always requires
    /// TLS (implicit TLS on port 465, mandatory STARTTLS on every other port, never the opportunistic mode that falls
    /// back to plaintext); <c>UseSsl = false</c> keeps the legacy opportunistic STARTTLS.
    /// </summary>
    public static SecureSocketOptions Resolve(MailKitOptions options)
    {
        if (options.SecureSocketOptions is { } explicitMode)
            return explicitMode;

        if (!options.UseSsl)
            return SecureSocketOptions.StartTlsWhenAvailable;

        return options.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
    }
}
