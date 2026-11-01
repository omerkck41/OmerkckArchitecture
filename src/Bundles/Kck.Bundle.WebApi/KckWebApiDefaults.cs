namespace Kck.Bundle.WebApi;

public sealed class KckWebApiDefaults
{
    public SecurityDefaults Security { get; set; } = new();
    public AspNetCoreDefaults AspNetCore { get; set; } = new();
}

public sealed class SecurityDefaults
{
    public JwtDefaults Jwt { get; set; } = new();
}

public sealed class JwtDefaults
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
}

public sealed class AspNetCoreDefaults
{
    public int RateLimitPermitLimit { get; set; } = 100;
    public int RateLimitWindowSeconds { get; set; } = 60;
    public bool SecurityHeaders { get; set; } = true;
    public string[]? CorsOrigins { get; set; }
}
