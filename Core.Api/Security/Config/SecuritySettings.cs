namespace Core.Api.Security.Config;

public class SecuritySettings
{
    public string[] AllowedIPs { get; set; } = Array.Empty<string>();
    public string[] AddCorsPolicy { get; set; } = Array.Empty<string>();
    public int RateLimit { get; set; } = 100;
}