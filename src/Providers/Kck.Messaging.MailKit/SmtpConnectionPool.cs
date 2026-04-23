using System.Threading.Channels;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;

namespace Kck.Messaging.MailKit;

/// <summary>
/// Pool of reusable <see cref="SmtpClient"/> connections.
/// SmtpClient is NOT thread-safe, so each borrower gets exclusive access.
/// </summary>
public sealed class SmtpConnectionPool : IAsyncDisposable
{
    private readonly MailKitOptions _options;
    private readonly Channel<SmtpClient> _pool;
    private readonly SemaphoreSlim _createLock = new(1, 1);
    private int _totalCreated;
    private int _disposed;

    public SmtpConnectionPool(IOptionsMonitor<MailKitOptions> options)
    {
        _options = options.CurrentValue;
        var maxSize = _options.PoolSize > 0 ? _options.PoolSize : 5;
        _pool = Channel.CreateBounded<SmtpClient>(new BoundedChannelOptions(maxSize)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    private bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    /// <summary>
    /// Borrows a connected and authenticated SmtpClient from the pool.
    /// The caller MUST return it via <see cref="ReturnAsync"/>.
    /// </summary>
    public async Task<SmtpClient> RentAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        if (_pool.Reader.TryRead(out var client))
        {
            if (client.IsConnected)
                return client;

            client.Dispose();
            Interlocked.Decrement(ref _totalCreated);
        }

        return await CreateClientAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a client to the pool for reuse. Disconnected clients are discarded.
    /// </summary>
    public async Task ReturnAsync(SmtpClient client)
    {
        if (IsDisposed || !client.IsConnected)
        {
            client.Dispose();
            Interlocked.Decrement(ref _totalCreated);
            return;
        }

        if (!_pool.Writer.TryWrite(client))
        {
            await client.DisconnectAsync(true).ConfigureAwait(false);
            client.Dispose();
            Interlocked.Decrement(ref _totalCreated);
        }
    }

    private async Task<SmtpClient> CreateClientAsync(CancellationToken ct)
    {
        var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl, ct).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(_options.UserName))
            {
                if (string.IsNullOrEmpty(_options.Password))
                    throw new InvalidOperationException(
                        "MailKitOptions.Password is required when UserName is set.");
                await client.AuthenticateAsync(_options.UserName, _options.Password, ct).ConfigureAwait(false);
            }

            Interlocked.Increment(ref _totalCreated);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            return;

        _pool.Writer.TryComplete();

        while (_pool.Reader.TryRead(out var client))
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true).ConfigureAwait(false);
            client.Dispose();
        }

        _createLock.Dispose();
    }
}
