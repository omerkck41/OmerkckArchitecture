using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Infrastructure.UniversalFTP.Services.Models;
using FluentFTP;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;

namespace Core.Infrastructure.UniversalFTP.Factories;

public class FtpConnectionPool
{
    private readonly FtpSettings _ftpSettings;
    private readonly ConcurrentBag<FtpClient> _availableClients;
    private readonly HashSet<FtpClient> _usedClients;
    private readonly SemaphoreSlim _semaphore;

    public FtpConnectionPool(IOptions<FtpSettings> options, int maxConnections = 10)
    {
        _ftpSettings = options.Value ?? throw new CustomArgumentException(nameof(options));
        _availableClients = new ConcurrentBag<FtpClient>();
        _usedClients = new HashSet<FtpClient>();
        _semaphore = new SemaphoreSlim(maxConnections);
    }

    public async Task<FtpClient> GetClientAsync()
    {
        await _semaphore.WaitAsync();

        if (!_availableClients.TryTake(out var client))
        {
            client = new FtpClient(_ftpSettings.Host, _ftpSettings.Port)
            {
                Credentials = new NetworkCredential(_ftpSettings.Username, _ftpSettings.Password),
                Config = new FtpConfig
                {
                    EncryptionMode = _ftpSettings.UseSsl ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None,
                    RetryAttempts = _ftpSettings.RetryCount
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
        if (client == null) throw new CustomArgumentException(nameof(client));

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