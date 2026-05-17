using Microsoft.Extensions.Options;

namespace Kck.Messaging.AmazonSes.DependencyInjection;

internal sealed class AmazonSesOptionsValidator : IValidateOptions<AmazonSesOptions>
{
    public ValidateOptionsResult Validate(string? name, AmazonSesOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Region))
            errors.Add(
                """
                  • Region: boş veya null
                    → Fix: opt.Region = "eu-central-1"
                """);

        // Explicit credentials optional (IAM role / env vars fallback desteklenir).
        // Ama biri varsa diğeri de zorunlu.
        var hasAccessKey = !string.IsNullOrWhiteSpace(options.AccessKey);
        var hasSecretKey = !string.IsNullOrWhiteSpace(options.SecretKey);
        if (hasAccessKey != hasSecretKey)
            errors.Add(
                """
                  • AccessKey / SecretKey: ikisi birlikte tanımlanmalı ya da ikisi de boş bırakılmalı
                    → Fix: opt.AccessKey = "AKIA...";  opt.SecretKey = "...";
                    → Alternatif: IAM role veya ortam değişkenleri (AWS_ACCESS_KEY_ID) kullan
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Messaging.AmazonSes] AmazonSesOptions geçersiz:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md
            """);
    }
}
