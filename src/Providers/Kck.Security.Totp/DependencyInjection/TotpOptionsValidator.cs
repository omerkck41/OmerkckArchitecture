using Microsoft.Extensions.Options;

namespace Kck.Security.Totp.DependencyInjection;

public sealed class TotpOptionsValidator : IValidateOptions<TotpOptions>
{
    private static readonly TimeSpan MaxFailedAttemptWindow = TimeSpan.FromDays(1);

    public ValidateOptionsResult Validate(string? name, TotpOptions options)
    {
        var errors = new List<string>();

        if (options.MaxFailedAttempts < 0)
            errors.Add(
                $"""
                  • MaxFailedAttempts: {options.MaxFailedAttempts} (must be 0, no limit, or more)
                    → Fix: opt.MaxFailedAttempts = 5
                """);

        // An invalid window would make every failed code throw instead of being counted, which turns the limit off.
        if (options.MaxFailedAttempts > 0
            && (options.FailedAttemptWindow <= TimeSpan.Zero || options.FailedAttemptWindow > MaxFailedAttemptWindow))
            errors.Add(
                $"""
                  • FailedAttemptWindow: {options.FailedAttemptWindow} (must be positive and at most {MaxFailedAttemptWindow})
                    → Fix: opt.FailedAttemptWindow = TimeSpan.FromMinutes(5)
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Security.Totp] TotpOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md
            """);
    }
}
