namespace Core.Infrastructure.UniversalFTP.Services.Models;

public class FtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 21;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
    public int RetryCount { get; set; } = 3;
    public int TimeoutInSeconds { get; set; } = 30;
    public TimeSpan? ConnectTimeout { get; set; }
    public TimeSpan? ReadTimeout { get; set; }
}