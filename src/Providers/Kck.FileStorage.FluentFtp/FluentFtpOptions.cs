namespace Kck.FileStorage.FluentFtp;

public sealed class FluentFtpOptions
{
    public required string Host { get; set; }
    public int Port { get; set; } = 21;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }

    /// <summary>
    /// Maximum number of pooled FTP connections. Values ≤ 0 fall back to 5.
    /// </summary>
    public int PoolSize { get; set; } = 5;
}
