# Kck.Security.Argon2

Argon2id-backed `IPasswordHasher` for OWASP-recommended password hashing with configurable memory, iteration, and parallelism parameters.

## Installation

```bash
dotnet add package Kck.Security.Argon2
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckArgon2(options =>
{
    options.Iterations = 3;
    options.MemorySize = 65536; // 64 MB
    options.DegreeOfParallelism = 1;
});

// Hash and verify passwords
public class UserService(IPasswordHasher hasher)
{
    public string HashPassword(string plaintext)
        => hasher.Hash(plaintext);

    public bool Verify(string plaintext, string hash)
        => hasher.Verify(plaintext, hash);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Iterations` | Number of Argon2 passes | `3` |
| `MemorySize` | Memory usage in kilobytes | `65536` |
| `DegreeOfParallelism` | Number of parallel lanes | `1` |
| `HashLength` | Output hash length in bytes | `32` |
| `SaltLength` | Salt length in bytes | `16` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
