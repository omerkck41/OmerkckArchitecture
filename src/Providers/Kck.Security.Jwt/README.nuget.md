# Kck.Security.Jwt

RS256-signed JWT `ITokenService` using RSA key pairs — generates and validates access tokens with configurable expiry and key source options.

## Installation

```bash
dotnet add package Kck.Security.Jwt
```

## Quick Start

```csharp
using Kck.Security.Abstractions.Token;
using Kck.Security.Jwt;

// Program.cs - RS256 with an RSA private key
builder.Services.AddKckJwt(options =>
{
    options.Issuer = "https://myapp.com";
    options.Audience = "api://myapp";
    options.AccessTokenExpiration = TimeSpan.FromMinutes(15);
    options.KeySource = RsaKeySource.Configuration;
    options.RsaKeyBase64 = Environment.GetEnvironmentVariable("JWT_RSA_KEY"); // PKCS#1 DER, Base64
});

// Create and validate tokens
public class AuthService(ITokenService tokens)
{
    public async Task<string> LoginAsync(Guid userId, string email, CancellationToken ct)
    {
        TokenResult token = await tokens.CreateAccessTokenAsync(
            new TokenRequest { UserId = userId.ToString(), Email = email, Roles = ["customer"] }, ct);
        return token.AccessToken;
    }

    public async Task<bool> IsValidAsync(string token, CancellationToken ct) =>
        (await tokens.ValidateTokenAsync(token, ct)).IsValid;
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Issuer` | JWT `iss` claim value | required |
| `Audience` | JWT `aud` claim value | required |
| `AccessTokenExpiration` | Token lifetime | `TimeSpan.FromMinutes(15)` |
| `KeySource` | `Configuration`, `File`, or `SecretsManager` | `Configuration` |
| `RsaKeyBase64` | Base64-encoded RSA private key PEM | required if `Configuration` |
| `RsaKeyPath` | Path to RSA private key PEM file | required if `File` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
