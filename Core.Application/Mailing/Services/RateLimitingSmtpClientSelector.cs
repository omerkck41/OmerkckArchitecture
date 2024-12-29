using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class RateLimitingSmtpClientSelector : ISmtpClientSelector
{
    private readonly List<SmtpClient> _smtpClients;
    private readonly Dictionary<SmtpClient, int> _sendCounts;
    private readonly int _maxSendsPerClient;

    public RateLimitingSmtpClientSelector(List<SmtpClient> smtpClients, int maxSendsPerClient = 100)
    {
        _smtpClients = smtpClients;
        _sendCounts = smtpClients.ToDictionary(client => client, client => 0);
        _maxSendsPerClient = maxSendsPerClient;
    }

    public SmtpClient GetNextClient()
    {
        var client = _smtpClients.FirstOrDefault(c => _sendCounts[c] < _maxSendsPerClient);

        if (client == null)
            throw new InvalidOperationException("No available SMTP clients within rate limits.");

        return client;
    }

    public void RegisterSend(SmtpClient smtpClient)
    {
        if (!_sendCounts.TryGetValue(smtpClient, out int value))
            throw new InvalidOperationException("SMTP client not registered.");

        _sendCounts[smtpClient] = ++value;
    }
}