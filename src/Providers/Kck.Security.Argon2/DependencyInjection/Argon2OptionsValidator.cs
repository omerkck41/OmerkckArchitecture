using Microsoft.Extensions.Options;

namespace Kck.Security.Argon2.DependencyInjection;

public sealed class Argon2OptionsValidator : IValidateOptions<Argon2Options>
{
    // OWASP minimums: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
    private const int MinMemoryKb = 19456;  // 19 MB (OWASP 2023 minimum)
    private const int MinIterations = 2;
    private const int MinParallelism = 1;
    private const int MinHashLength = 16;
    private const int MinSaltLength = 16;

    public ValidateOptionsResult Validate(string? name, Argon2Options options)
    {
        var errors = new List<string>();

        if (options.MemorySize < MinMemoryKb)
            errors.Add(
                $"""
                  • MemorySize: {options.MemorySize} KB (OWASP minimum {MinMemoryKb} KB = 19 MB)
                    → Fix: opt.MemorySize = 65536   // 64 MB (önerilen)
                """);

        if (options.Iterations < MinIterations)
            errors.Add(
                $"""
                  • Iterations: {options.Iterations} (minimum {MinIterations})
                    → Fix: opt.Iterations = 3
                """);

        if (options.DegreeOfParallelism < MinParallelism)
            errors.Add(
                $"""
                  • DegreeOfParallelism: {options.DegreeOfParallelism} (minimum {MinParallelism})
                    → Fix: opt.DegreeOfParallelism = 1
                """);

        if (options.HashLength < MinHashLength)
            errors.Add(
                $"""
                  • HashLength: {options.HashLength} bytes (minimum {MinHashLength})
                    → Fix: opt.HashLength = 32
                """);

        if (options.SaltLength < MinSaltLength)
            errors.Add(
                $"""
                  • SaltLength: {options.SaltLength} bytes (minimum {MinSaltLength})
                    → Fix: opt.SaltLength = 16
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Security.Argon2] Argon2Options OWASP minimum değerlerin altında:
            {string.Join(Environment.NewLine, errors)}
              Ref: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md
            """);
    }
}
