namespace Kck.FileStorage.AwsS3;

/// <summary>
/// Configuration for <see cref="AwsS3StorageService"/>.
/// Omit <see cref="AccessKey"/> / <see cref="SecretKey"/> to use IAM role or environment-variable credentials.
/// </summary>
public sealed class AwsS3Options
{
    /// <summary>AWS region (e.g. <c>eu-central-1</c>). Required.</summary>
    public required string Region { get; set; }

    /// <summary>S3 bucket name. Required.</summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// AWS access key ID. Leave <see langword="null"/> to use IAM role /
    /// <c>AWS_ACCESS_KEY_ID</c> environment variable.
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// AWS secret access key. Must be set when <see cref="AccessKey"/> is provided.
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Optional key prefix prepended to all object paths.
    /// Useful for multi-tenant isolation within a single bucket.
    /// </summary>
    public string? KeyPrefix { get; set; }
}
