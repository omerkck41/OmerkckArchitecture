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
    public string Algorithm { get; set; } = "RS256";
}
