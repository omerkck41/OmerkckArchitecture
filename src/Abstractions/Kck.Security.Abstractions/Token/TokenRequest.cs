namespace Kck.Security.Abstractions.Token;

/// <summary>Token creation request containing user identity and claims.</summary>
public sealed record TokenRequest
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public string? Name { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyDictionary<string, string> CustomClaims { get; init; } = new Dictionary<string, string>();
}
