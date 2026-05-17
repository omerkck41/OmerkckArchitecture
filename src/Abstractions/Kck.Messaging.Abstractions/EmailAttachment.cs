namespace Kck.Messaging.Abstractions;

/// <summary>
/// Represents a file attachment that can be included in an outgoing email message.
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>Gets the display name of the attached file, including its extension.</summary>
    public required string FileName { get; init; }

    /// <summary>Gets the stream containing the binary content of the attachment.</summary>
    public required Stream Content { get; init; }

    /// <summary>Gets the MIME content-type of the attachment. Defaults to "application/octet-stream".</summary>
    public string ContentType { get; init; } = "application/octet-stream";
}
