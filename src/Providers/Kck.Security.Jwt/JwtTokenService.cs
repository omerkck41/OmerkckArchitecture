using System.Security.Claims;
using System.Security.Cryptography;
using Kck.Security.Abstractions.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using KckTokenValidationResult = Kck.Security.Abstractions.Token.TokenValidationResult;

namespace Kck.Security.Jwt;

/// <summary>RS256 JWT token service. Throws if signing key is not configured.</summary>
public sealed class JwtTokenService : ITokenService, IDisposable
{
    private readonly JwtOptions _options;
    private readonly ITokenBlacklistService? _blacklist;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly Lazy<RsaSecurityKey> _signingKey;

    public JwtTokenService(
        IOptionsMonitor<JwtOptions> options,
        ILogger<JwtTokenService> logger,
        ITokenBlacklistService? blacklist = null)
    {
        _options = options.CurrentValue;
        _logger = logger;
        _blacklist = blacklist;
        _signingKey = new Lazy<RsaSecurityKey>(LoadSigningKey);
    }

    public Task<TokenResult> CreateAccessTokenAsync(TokenRequest request, CancellationToken ct = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, request.UserId),
            new(ClaimTypes.Email, request.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        if (request.Name is not null)
            claims.Add(new Claim(ClaimTypes.Name, request.Name));

        foreach (var role in request.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var (key, value) in request.CustomClaims)
            claims.Add(new Claim(key, value));

        var now = DateTime.UtcNow;
        var expires = now.Add(_options.AccessTokenExpiration);

        var credentials = new SigningCredentials(_signingKey.Value, SecurityAlgorithms.RsaSha256);

        var handler = new JsonWebTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            NotBefore = now,
            Expires = expires,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials
        };

        var tokenString = handler.CreateToken(descriptor);

        _logger.LogInformation("Access token created for user {UserId}", request.UserId);

        return Task.FromResult(new TokenResult
        {
            AccessToken = tokenString,
            ExpiresAt = expires
        });
    }

    public Task<string> CreateRefreshTokenAsync(CancellationToken ct = default)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Task.FromResult(Convert.ToBase64String(randomBytes));
    }

    public async Task<KckTokenValidationResult> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        var handler = new JsonWebTokenHandler();

        try
        {
            // 1. FIRST validate signature + issuer + audience + lifetime
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey.Value
            };

            var principal = await handler.ValidateTokenAsync(token, validationParams);

            if (!principal.IsValid)
                return new KckTokenValidationResult { IsValid = false, ErrorMessage = "Token validation failed" };

            // 2. AFTER signature is valid, THEN check blacklist
            var jti = principal.Claims.TryGetValue(JwtRegisteredClaimNames.Jti, out var jtiObj)
                ? jtiObj?.ToString() : null;

            if (_blacklist is not null && !string.IsNullOrEmpty(jti))
            {
                if (await _blacklist.IsRevokedAsync(jti, ct))
                {
                    _logger.LogInformation("Token {Jti} is revoked", jti);
                    return new KckTokenValidationResult { IsValid = false, ErrorMessage = "Token has been revoked" };
                }
            }

            // 3. Return claims
            var claims = principal.Claims;
            var userId = claims.TryGetValue(ClaimTypes.NameIdentifier, out var uid) ? uid?.ToString() : null;
            var email = claims.TryGetValue(ClaimTypes.Email, out var em) ? em?.ToString() : null;
            var roles = claims.Where(c => c.Key == ClaimTypes.Role).Select(c => c.Value?.ToString() ?? "").ToList();

            return new KckTokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Email = email,
                Roles = roles
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return new KckTokenValidationResult { IsValid = false, ErrorMessage = "Token validation failed" };
        }
    }

    /// <summary>
    /// Extracts claims from a token WITHOUT signature validation.
    /// Do NOT use for authorization decisions — use <see cref="ValidateTokenAsync"/> instead.
    /// Intended for debug/logging purposes only.
    /// </summary>
    [Obsolete("Use ValidateTokenAsync. GetClaimsFromToken does NOT verify signature — unsafe for authorization. For debug/logging only.", DiagnosticId = "KCK0001")]
    public IReadOnlyDictionary<string, string> GetClaimsFromToken(string token)
    {
        var jwt = new JsonWebToken(token);
        return jwt.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => string.Join(",", g.Select(c => c.Value)));
    }

    private RsaSecurityKey LoadSigningKey()
    {
        var rsa = RSA.Create();

        switch (_options.KeySource)
        {
            case RsaKeySource.Configuration:
                if (string.IsNullOrWhiteSpace(_options.RsaKeyBase64))
                    throw new InvalidOperationException(
                        "JWT signing key is not configured. Set JwtOptions.RsaKeyBase64 or use a different RsaKeySource. " +
                        "Hardcoded fallback keys are not allowed.");
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(_options.RsaKeyBase64), out _);
                break;

            case RsaKeySource.File:
                if (string.IsNullOrWhiteSpace(_options.RsaKeyPath) || !System.IO.File.Exists(_options.RsaKeyPath))
                    throw new InvalidOperationException(
                        $"RSA key file not found at '{_options.RsaKeyPath}'. Provide a valid PEM file path.");
                var pem = System.IO.File.ReadAllText(_options.RsaKeyPath);
                rsa.ImportFromPem(pem);
                break;

            case RsaKeySource.SecretsManager:
                throw new InvalidOperationException(
                    "RsaKeySource.SecretsManager requires manual key loading via ISecretsManager. " +
                    "Use the AddKckJwt overload that accepts a key provider delegate.");

            default:
                throw new InvalidOperationException($"Unsupported RsaKeySource: {_options.KeySource}");
        }

        _logger.LogInformation("RSA signing key loaded via {KeySource}", _options.KeySource);
        return new RsaSecurityKey(rsa);
    }

    public void Dispose()
    {
        if (_signingKey.IsValueCreated && _signingKey.Value is { Rsa: { } rsa })
            rsa.Dispose();
    }
}
