# Kck.Security.Jwt

RS256-signed JWT `ITokenService` using RSA key pairs — generates and validates access tokens with configurable expiry and key source options.

## Installation

```bash
dotnet add package Kck.Security.Jwt
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckJwt(options =>
{
    options.Issuer = "https://myapp.com";
    options.Audience = "api://myapp";
    options.AccessTokenExpiration = TimeSpan.FromMinutes(15);
    options.KeySource = RsaKeySource.Configuration;
    options.RsaKeyBase64 = Environment.GetEnvironmentVariable("JWT_RSA_KEY")!;
});

// Generate and validate tokens
public class AuthService(ITokenService tokens)
{
    public async Task<string> LoginAsync(User user, CancellationToken ct)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        return await tokens.GenerateAsync(claims, ct);
    }
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
