using Microsoft.Extensions.Options;

namespace Kck.FileStorage.FluentFtp.DependencyInjection;

internal sealed class FluentFtpOptionsValidator : IValidateOptions<FluentFtpOptions>
{
    public ValidateOptionsResult Validate(string? name, FluentFtpOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
            errors.Add(
                """
                  • Host: boş veya null
                    → Fix: opt.Host = "ftp.example.com"
                """);

        if (options.Port is < 1 or > 65535)
            errors.Add(
                $"""
                  • Port: {options.Port} geçersiz (1–65535 arası olmalı)
                    → Fix: opt.Port = 21   (standart FTP) veya 22 (SFTP)
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.FileStorage.FluentFtp] FluentFtpOptions geçersiz:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md
            """);
    }
}
