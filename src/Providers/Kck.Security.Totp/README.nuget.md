# Kck.Security.Totp

TOTP-based multi-factor authentication provider compatible with Google Authenticator and Authy — implements `IMfaProvider` with QR setup support.

## Installation

```bash
dotnet add package Kck.Security.Totp
```

## Quick Start

```csharp
using Kck.Security.Abstractions.Mfa;

// Program.cs - replay protection needs IMemoryCache
builder.Services.AddMemoryCache();
builder.Services.AddKckTotp();

// MFA setup and verification
public class MfaService(IMfaProvider mfa)
{
    // Show ProvisioningUri as a QR code; store SecretKey encrypted with the user
    public Task<MfaSetupResult> BeginSetupAsync(string email, CancellationToken ct) =>
        mfa.GenerateSetupAsync(email, issuer: "MyApp", ct);

    // Rate-limit the endpoint that calls this: 6-digit codes can be guessed
    public Task<bool> VerifyAsync(string storedSecretKey, string code, CancellationToken ct) =>
        mfa.ValidateCodeAsync(storedSecretKey, code, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `CodeLength` | Digits in a code | `6` |
| `StepSeconds` | Time step in seconds | `30` |
| `VerificationWindow` | Steps accepted before and after the current one | `1` |

The issuer is passed to `GenerateSetupAsync`. Replay protection needs `services.AddMemoryCache()`.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
