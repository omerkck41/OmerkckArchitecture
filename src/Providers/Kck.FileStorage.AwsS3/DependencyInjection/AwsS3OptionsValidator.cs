using Microsoft.Extensions.Options;

namespace Kck.FileStorage.AwsS3.DependencyInjection;

/// <summary>Validates <see cref="AwsS3Options"/> at application startup.</summary>
public sealed class AwsS3OptionsValidator : IValidateOptions<AwsS3Options>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, AwsS3Options options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Region))
            errors.Add(
                """
                  • Region: boş veya null
                    → Fix: opt.Region = "eu-central-1";
                """);

        if (string.IsNullOrWhiteSpace(options.BucketName))
            errors.Add(
                """
                  • BucketName: boş veya null
                    → Fix: opt.BucketName = "my-bucket";
                """);

        var hasKey = !string.IsNullOrWhiteSpace(options.AccessKey);
        var hasSecret = !string.IsNullOrWhiteSpace(options.SecretKey);
        if (hasKey != hasSecret)
            errors.Add(
                """
                  • AccessKey / SecretKey: ikisi birlikte tanımlanmalı ya da ikisi de boş bırakılmalı
                    → IAM role kullanıyorsanız her ikisini de null bırakın
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.FileStorage.AwsS3] AwsS3Options geçersiz:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md
            """);
    }
}
