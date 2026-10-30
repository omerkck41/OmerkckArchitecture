namespace Kck.Bundle.MinimalApi;

public sealed class KckMinimalApiDefaults
{
    public SecurityDefaults Security { get; set; } = new();
}

public sealed class SecurityDefaults
{
    public JwtDefaults Jwt { get; set; } = new();
}

public sealed class JwtDefaults
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    [Obsolete(
        "Not read by AddKckMinimalApiDefaults: " +
        "Kck.Security.Jwt always signs with RS256. Removed in v5.0.",
        DiagnosticId = "KCK0002",
        UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
    public string Algorithm { get; set; } = "RS256";
}
