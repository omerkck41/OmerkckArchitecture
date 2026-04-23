namespace Kck.Messaging.MailKit;

public sealed class MailKitOptions
{
    public required string Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public int PoolSize { get; set; } = 5;
}
