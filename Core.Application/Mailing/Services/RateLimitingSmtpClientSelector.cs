using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using System.Net.Mail;

namespace Core.Application.Mailing.Services;

public class RateLimitingSmtpClientSelector : ISmtpClientSelector
{
    private readonly List<SmtpClient> _smtpClients;
    private readonly Dictionary<SmtpClient, int> _sendCounts;
    private readonly int _maxSendsPerClient;
    private readonly object _lock = new object();

    public RateLimitingSmtpClientSelector(List<SmtpClient> smtpClients, int maxSendsPerClient = 100)
    {
        _smtpClients = smtpClients;
        _sendCounts = smtpClients.ToDictionary(client => client, client => 0);
        _maxSendsPerClient = maxSendsPerClient;
    }

    public SmtpClient GetNextClient()
    {
        lock (_lock)
        {
            var client = _smtpClients.FirstOrDefault(c => _sendCounts[c] < _maxSendsPerClient);
            if (client != null)
            {
                _sendCounts[client]++;
                return client;
            }
            throw new CustomException("No available SMTP client within the rate limit.");
        }
    }

    public void RegisterSend(SmtpClient smtpClient)
    {
        if (!_sendCounts.TryGetValue(smtpClient, out int value))
            throw new CustomException("SMTP client not registered.");

        _sendCounts[smtpClient] = value + 1;
    }
}