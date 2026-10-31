using Microsoft.Extensions.Options;

namespace Kck.FileStorage.FluentFtp.DependencyInjection;

public sealed class FluentFtpOptionsValidator : IValidateOptions<FluentFtpOptions>
{
    public ValidateOptionsResult Validate(string? name, FluentFtpOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
            errors.Add(
                """
                  • Host: null or empty
                    → Fix: opt.Host = "ftp.example.com"
                """);

        if (options.Port is < 1 or > 65535)
            errors.Add(
                $"""
                  • Port: {options.Port} is invalid (must be between 1 and 65535)
                    → Fix: opt.Port = 21   (standard FTP) or 22 (SFTP)
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.FileStorage.FluentFtp] FluentFtpOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md
            """);
    }
}
