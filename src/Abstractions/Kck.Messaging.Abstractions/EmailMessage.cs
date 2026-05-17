namespace Kck.Messaging.Abstractions;

/// <summary>
/// Represents a complete email message including sender, recipients, subject, body, and optional attachments.
/// </summary>
public sealed class EmailMessage
{
    /// <summary>Gets the sender's email address.</summary>
    public required string From { get; init; }

    /// <summary>Gets the optional display name shown alongside the sender address.</summary>
    public string? FromName { get; init; }

    /// <summary>Gets the subject line of the email.</summary>
    public required string Subject { get; init; }

    /// <summary>Gets the body content of the email.</summary>
    public required string Body { get; init; }

    /// <summary>Gets a value indicating whether <see cref="Body"/> contains HTML markup. Defaults to <see langword="true"/>.</summary>
    public bool IsHtml { get; init; } = true;

    /// <summary>Gets the list of recipients (To, Cc, Bcc) for this message.</summary>
    public IReadOnlyList<EmailRecipient> Recipients { get; init; } = [];

    /// <summary>Gets the list of file attachments included with this message.</summary>
    public IReadOnlyList<EmailAttachment> Attachments { get; init; } = [];
}
