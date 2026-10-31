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
                  • Region: null or empty
                    → Fix: opt.Region = "eu-central-1";
                """);

        if (string.IsNullOrWhiteSpace(options.BucketName))
            errors.Add(
                """
                  • BucketName: null or empty
                    → Fix: opt.BucketName = "my-bucket";
                """);

        var hasKey = !string.IsNullOrWhiteSpace(options.AccessKey);
        var hasSecret = !string.IsNullOrWhiteSpace(options.SecretKey);
        if (hasKey != hasSecret)
            errors.Add(
                """
                  • AccessKey / SecretKey: set both or leave both empty
                    → If you use an IAM role, leave both null
                """);

        if (!string.IsNullOrWhiteSpace(options.ServiceUrl)
            && !(Uri.TryCreate(options.ServiceUrl, UriKind.Absolute, out var serviceUri)
                 && (serviceUri.Scheme == Uri.UriSchemeHttp || serviceUri.Scheme == Uri.UriSchemeHttps)))
            errors.Add(
                """
                  • ServiceUrl: must be an absolute http(s) address
                    → Fix: opt.ServiceUrl = "http://localhost:9000"; (leave null for Amazon S3)
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.FileStorage.AwsS3] AwsS3Options is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md
            """);
    }
}
