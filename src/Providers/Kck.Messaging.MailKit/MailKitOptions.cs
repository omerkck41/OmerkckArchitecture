using MailKit.Security;

namespace Kck.Messaging.MailKit;

/// <summary>
/// Configuration options for the MailKit SMTP email provider and its connection pool.
/// </summary>
public sealed class MailKitOptions
{
    /// <summary>Gets or sets the SMTP server hostname or IP address.</summary>
    public required string Host { get; set; }

    /// <summary>Gets or sets the SMTP port; defaults to <c>587</c> (STARTTLS).</summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets or sets a value indicating whether TLS is required; defaults to <see langword="true"/>.
    /// Legacy switch, consulted only when <see cref="SecureSocketOptions"/> is <see langword="null"/>:
    /// <see langword="false"/> means STARTTLS when the server offers it (opportunistic, may end up in plaintext);
    /// <see langword="true"/> requires TLS: implicit TLS on port 465 and mandatory STARTTLS on every other port.
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the explicit TLS mode of the SMTP connection, for example
    /// <see cref="global::MailKit.Security.SecureSocketOptions.SslOnConnect"/> for implicit TLS on a custom port.
    /// Defaults to <see langword="null"/>: the mode is derived from <see cref="UseSsl"/> and <see cref="Port"/>.
    /// </summary>
    public SecureSocketOptions? SecureSocketOptions { get; set; }

    /// <summary>Gets or sets the SMTP authentication username; leave <see langword="null"/> for unauthenticated relay.</summary>
    public string? UserName { get; set; }

    /// <summary>Gets or sets the SMTP authentication password paired with <see cref="UserName"/>.</summary>
    public string? Password { get; set; }

    /// <summary>Gets or sets the maximum number of SMTP connections kept alive in the pool; defaults to <c>5</c>.</summary>
    public int PoolSize { get; set; } = 5;
}
