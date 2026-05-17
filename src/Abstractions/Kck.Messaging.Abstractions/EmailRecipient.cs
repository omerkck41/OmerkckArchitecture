namespace Kck.Messaging.Abstractions;

/// <summary>
/// Represents an email recipient with an address, optional display name, and delivery role (To, Cc, or Bcc).
/// </summary>
public sealed record EmailRecipient(string Email, string? Name = null, RecipientType Type = RecipientType.To);

/// <summary>
/// Specifies the role of an email recipient in the message delivery.
/// </summary>
public enum RecipientType
{
    /// <summary>Primary recipient; address appears in the To field.</summary>
    To,

    /// <summary>Carbon copy recipient; address appears in the Cc field.</summary>
    Cc,

    /// <summary>Blind carbon copy recipient; address is hidden from other recipients.</summary>
    Bcc
}
