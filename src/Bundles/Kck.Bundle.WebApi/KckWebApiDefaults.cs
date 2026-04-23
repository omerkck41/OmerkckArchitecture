namespace Kck.Bundle.WebApi;

public sealed class KckWebApiDefaults
{
    public PersistenceDefaults Persistence { get; set; } = new();
    public CachingDefaults Caching { get; set; } = new();
    public SecurityDefaults Security { get; set; } = new();
    public ObservabilityDefaults Observability { get; set; } = new();
    public AspNetCoreDefaults AspNetCore { get; set; } = new();
}

public sealed class PersistenceDefaults
{
    public string Provider { get; set; } = "EntityFramework";
    public string? ConnectionString { get; set; }
}

public sealed class CachingDefaults
{
    public string Provider { get; set; } = "InMemory";
    public string? ConnectionString { get; set; }
}

public sealed class SecurityDefaults
{
    public JwtDefaults Jwt { get; set; } = new();
    public string Hashing { get; set; } = "Argon2";
}

public sealed class JwtDefaults
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string Algorithm { get; set; } = "RS256";
}

public sealed class ObservabilityDefaults
{
    public bool Tracing { get; set; } = true;
    public bool Metrics { get; set; } = true;
    public string? OtlpEndpoint { get; set; }
    public string? ServiceName { get; set; }
}

public sealed class AspNetCoreDefaults
{
    public int RateLimitPermitLimit { get; set; } = 100;
    public int RateLimitWindowSeconds { get; set; } = 60;
    public bool SecurityHeaders { get; set; } = true;
    public string[]? CorsOrigins { get; set; }
}
