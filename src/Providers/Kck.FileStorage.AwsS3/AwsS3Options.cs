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

    /// <summary>
    /// Endpoint of an S3-compatible service (e.g. MinIO <c>http://localhost:9000</c>, Cloudflare R2, Wasabi).
    /// Leave <see langword="null"/> for Amazon S3. When set, <see cref="Region"/> is used only to sign requests.
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Use path-style URLs (<c>https://host/bucket/key</c>) instead of virtual-hosted style
    /// (<c>https://bucket.host/key</c>). Most self-hosted S3-compatible services (e.g. MinIO) need this.
    /// </summary>
    public bool ForcePathStyle { get; set; }

    /// <summary>
    /// Compute and validate checksums only when the operation requires them. AWS SDK v4 adds CRC32
    /// checksums to every request by default, which some S3-compatible services reject; enable this for them.
    /// </summary>
    public bool ChecksumWhenRequired { get; set; }
}
