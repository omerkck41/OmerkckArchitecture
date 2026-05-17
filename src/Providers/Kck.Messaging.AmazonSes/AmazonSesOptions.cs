namespace Kck.Messaging.AmazonSes;

/// <summary>
/// Configuration options for the Amazon SES email provider.
/// </summary>
public sealed class AmazonSesOptions
{
    /// <summary>Gets or sets the AWS region system name (e.g. <c>us-east-1</c>) for the SES endpoint.</summary>
    public required string Region { get; set; }

    /// <summary>Gets or sets the AWS access key ID; when omitted the default credential chain is used.</summary>
    public string? AccessKey { get; set; }

    /// <summary>Gets or sets the AWS secret access key paired with <see cref="AccessKey"/>.</summary>
    public string? SecretKey { get; set; }
}
