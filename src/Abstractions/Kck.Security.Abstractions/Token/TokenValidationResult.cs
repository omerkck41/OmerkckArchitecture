namespace Kck.Security.Abstractions.Token;

/// <summary>Result of token validation.</summary>
public sealed record TokenValidationResult
{
    public bool IsValid { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();
    public string? ErrorMessage { get; init; }
}
