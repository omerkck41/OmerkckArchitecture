namespace Core.Api.Security.Config;

public class SecuritySettings
{
    public List<string> AllowedIPs { get; set; } = new List<string>();
    public List<string> AddCorsPolicy { get; set; } = new List<string>();
    public int RateLimit { get; set; } = 100;           // Rate Limiting için eklendi
    public int MaxLoginAttempts { get; set; } = 5;      // Brute Force için eklendi
    public int LockoutTime { get; set; } = 5;           // Brute Force için eklendi (dakika cinsinden)
    public bool EnforceHttps { get; set; } = true;
}