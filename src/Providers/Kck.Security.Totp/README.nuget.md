# Kck.Security.Totp

TOTP-based multi-factor authentication provider compatible with Google Authenticator and Authy — implements `IMfaProvider` with QR setup support.

## Installation

```bash
dotnet add package Kck.Security.Totp
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckTotp();

// Generate MFA setup for a user
public class MfaController(IMfaProvider mfa)
{
    [HttpPost("setup")]
    public async Task<SetupResult> Setup(string userId, CancellationToken ct)
    {
        var setup = await mfa.GenerateSetupAsync(userId, ct);
        // setup.QrCodeUri → render as QR code for the user's authenticator app
        // setup.SecretKey → store encrypted in the database
        return setup;
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(string userId, string code, CancellationToken ct)
    {
        var valid = await mfa.VerifyAsync(userId, code, ct);
        return valid ? Ok() : Unauthorized();
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Issuer` | App name shown in authenticator apps | assembly name |
| `StepSeconds` | TOTP time-step in seconds | `30` |
| `Digits` | OTP digit count | `6` |
| `AllowedClockDriftSteps` | Clock skew tolerance (steps) | `1` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
