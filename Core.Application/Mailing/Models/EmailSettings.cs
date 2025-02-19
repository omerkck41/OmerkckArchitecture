namespace Core.Application.Mailing.Models;

public class EmailSettings
{
    public string DefaultFromAddress { get; set; } = string.Empty;
    public string DefaultFromName { get; set; } = string.Empty;
    public string SendGridApiKey { get; set; } = string.Empty;
    public string AwsRegion { get; set; } = string.Empty;
    public string AwsAccessKey { get; set; } = string.Empty;
    public string AwsSecretKey { get; set; } = string.Empty;

    public List<SmtpServerSettings> SmtpServers { get; set; } = [];
    public int MaxSendsPerClient { get; set; } = 100; // Varsayılan değer
    public string PreferredProvider { get; set; } = "Smtp"; // Varsayılan sağlayıcı
}