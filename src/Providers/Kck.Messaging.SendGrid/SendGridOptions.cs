namespace Kck.Messaging.SendGrid;

/// <summary>
/// Configuration options for the SendGrid email provider.
/// </summary>
public sealed class SendGridOptions
{
    /// <summary>Gets or sets the SendGrid API key used to authenticate requests.</summary>
    public required string ApiKey { get; set; }
}
