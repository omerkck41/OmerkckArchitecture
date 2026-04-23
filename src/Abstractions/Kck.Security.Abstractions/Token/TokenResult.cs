namespace Kck.Security.Abstractions.Token;

/// <summary>Result of token creation.</summary>
public sealed record TokenResult
{
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
