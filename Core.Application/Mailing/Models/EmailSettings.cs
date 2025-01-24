namespace Core.Application.Mailing.Models;

public class EmailSettings
{
    public string SendGridApiKey { get; set; } = string.Empty;
    public List<SmtpServerSettings> SmtpServers { get; set; } = new();
    public int MaxSendsPerClient { get; set; } = 100; // Varsayılan değer
    public string PreferredProvider { get; set; } = "Smtp"; // Varsayılan sağlayıcı
}