using Core.Infrastructure.UniversalFTP.Services.Models;
using FluentFTP;
using System.Collections.Concurrent;
using System.Net;

namespace Core.Infrastructure.UniversalFTP.Factories;

public class FtpConnectionPool
{
    private readonly FtpSettings _settings;
    private readonly ConcurrentBag<FtpClient> _availableClients;
    private readonly HashSet<FtpClient> _usedClients;
    private readonly SemaphoreSlim _semaphore;

    public FtpConnectionPool(FtpSettings settings, int maxConnections = 10)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _availableClients = new ConcurrentBag<FtpClient>();
        _usedClients = new HashSet<FtpClient>();
        _semaphore = new SemaphoreSlim(maxConnections);
    }

    public async Task<FtpClient> GetClientAsync()
    {
        await _semaphore.WaitAsync();

        if (!_availableClients.TryTake(out var client))
        {
            client = new FtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                Config = new FtpConfig
                {
                    EncryptionMode = _settings.UseSsl ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None,
                    RetryAttempts = _settings.RetryCount
                }
            };

            await Task.Run(() => client.Connect());
        }

        lock (_usedClients)
        {
            _usedClients.Add(client);
        }

        return client;
    }

    public void ReleaseClient(FtpClient client)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));

        lock (_usedClients)
        {
            if (_usedClients.Remove(client))
            {
                _availableClients.Add(client);
                _semaphore.Release();
            }
        }
    }
}
