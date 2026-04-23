namespace Kck.Messaging.Abstractions;

public sealed record EmailRecipient(string Email, string? Name = null, RecipientType Type = RecipientType.To);

public enum RecipientType
{
    To,
    Cc,
    Bcc
}
