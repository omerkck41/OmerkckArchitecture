namespace Core.Application.Mailing.Services;

public class EmailRecipient
{
    public string? Name { get; set; }
    public string Email { get; set; } = default!;
    public RecipientType Type { get; set; } // To, Cc, Bcc
}

public enum RecipientType
{
    To,
    Cc,
    Bcc
}
