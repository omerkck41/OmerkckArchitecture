namespace Kck.Bundle.WebApi;

public sealed class KckWebApiDefaults
{
    [Obsolete(
        "Not read by AddKckWebApiDefaults: " +
        "register persistence in code with AddKckPersistence(...). Removed in v5.0.",
        DiagnosticId = "KCK0002",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    public PersistenceDefaults Persistence { get; set; } = new();
    [Obsolete(
        "Not read by AddKckWebApiDefaults: " +
        "the bundle always registers the in-memory cache; register another provider in code. Removed in v5.0.",
        DiagnosticId = "KCK0002",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    public CachingDefaults Caching { get; set; } = new();
    public SecurityDefaults Security { get; set; } = new();
    [Obsolete(
        "Not read by AddKckWebApiDefaults: " +
        "configure OpenTelemetry in code with AddKckObservability(o => o.UseOpenTelemetry(...)). Removed in v5.0.",
        DiagnosticId = "KCK0002",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
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
    [Obsolete(
        "Not read by AddKckWebApiDefaults: " +
        "the bundle always registers Argon2. Removed in v5.0.",
        DiagnosticId = "KCK0002",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    public string Hashing { get; set; } = "Argon2";
}

public sealed class JwtDefaults
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    [Obsolete(
        "Not read by AddKckWebApiDefaults: " +
        "Kck.Security.Jwt always signs with RS256. Removed in v5.0.",
        DiagnosticId = "KCK0002",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
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
