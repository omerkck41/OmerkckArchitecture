namespace Kck.Messaging.Abstractions;

public sealed class EmailMessage
{
    public required string From { get; init; }
    public string? FromName { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; } = true;
    public IReadOnlyList<EmailRecipient> Recipients { get; init; } = [];
    public IReadOnlyList<EmailAttachment> Attachments { get; init; } = [];
}
