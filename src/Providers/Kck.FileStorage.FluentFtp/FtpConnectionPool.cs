using System.Threading.Channels;
using FluentFTP;
using Microsoft.Extensions.Options;

namespace Kck.FileStorage.FluentFtp;

/// <summary>
/// Pool of reusable <see cref="AsyncFtpClient"/> connections.
/// <see cref="AsyncFtpClient"/> is NOT safe for concurrent operations, so each
/// borrower gets exclusive access and MUST return the client via <see cref="ReturnAsync"/>.
/// </summary>
public sealed class FtpConnectionPool : IAsyncDisposable
{
    private readonly FluentFtpOptions _options;
    private readonly Channel<AsyncFtpClient> _pool;
    private int _totalCreated;
    private int _disposed;

    public FtpConnectionPool(IOptionsMonitor<FluentFtpOptions> options)
    {
        _options = options.CurrentValue;
        var maxSize = _options.PoolSize > 0 ? _options.PoolSize : 5;
        _pool = Channel.CreateBounded<AsyncFtpClient>(new BoundedChannelOptions(maxSize)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    private bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    /// <summary>
    /// Borrows a connected <see cref="AsyncFtpClient"/> from the pool.
    /// The caller MUST return it via <see cref="ReturnAsync"/>.
    /// </summary>
    public async Task<AsyncFtpClient> RentAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        while (_pool.Reader.TryRead(out var pooled))
        {
            if (pooled.IsConnected)
                return pooled;

            pooled.Dispose();
            Interlocked.Decrement(ref _totalCreated);
        }

        return await CreateClientAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns a client to the pool for reuse. Disconnected clients are discarded.
    /// </summary>
    public async Task ReturnAsync(AsyncFtpClient client)
    {
        if (IsDisposed || !client.IsConnected)
        {
            await DisposeClientAsync(client).ConfigureAwait(false);
            return;
        }

        if (!_pool.Writer.TryWrite(client))
            await DisposeClientAsync(client).ConfigureAwait(false);
    }

    private async Task<AsyncFtpClient> CreateClientAsync(CancellationToken ct)
    {
        var client = new AsyncFtpClient(_options.Host, _options.Username, _options.Password, _options.Port);
        if (_options.UseSsl)
            client.Config.EncryptionMode = FtpEncryptionMode.Explicit;

        try
        {
            await client.Connect(ct).ConfigureAwait(false);
            Interlocked.Increment(ref _totalCreated);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    private async Task DisposeClientAsync(AsyncFtpClient client)
    {
        try
        {
            if (client.IsConnected)
                await client.Disconnect().ConfigureAwait(false);
        }
        catch
        {
            // best-effort disconnect; the client is being discarded anyway
        }

        client.Dispose();
        Interlocked.Decrement(ref _totalCreated);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            return;

        _pool.Writer.TryComplete();

        while (_pool.Reader.TryRead(out var client))
            await DisposeClientAsync(client).ConfigureAwait(false);
    }
}
