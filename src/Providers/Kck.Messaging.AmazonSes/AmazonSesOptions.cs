namespace Kck.Messaging.AmazonSes;

public sealed class AmazonSesOptions
{
    public required string Region { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
}
