using FluentAssertions;
using Kck.Messaging.MailKit;
using Kck.Testing;
using Xunit;

namespace Kck.Messaging.MailKit.Tests;

public class SmtpConnectionPoolTests
{
    private static SmtpConnectionPool CreatePool(int poolSize = 5)
    {
        var options = new StaticOptionsMonitor<MailKitOptions>(new MailKitOptions
        {
            Host = "smtp.invalid.test",
            Port = 2525,
            UseSsl = false,
            PoolSize = poolSize
        });
        return new SmtpConnectionPool(options);
    }

    [Fact]
    public async Task RentAsync_WhenDisposed_ShouldThrowObjectDisposed()
    {
        var pool = CreatePool();
        await pool.DisposeAsync();

        var act = async () => await pool.RentAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_ShouldNotThrow()
    {
        var pool = CreatePool();

        await pool.DisposeAsync();
        var act = async () => await pool.DisposeAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_WithZeroPoolSize_ShouldFallbackToDefault()
    {
        var options = new StaticOptionsMonitor<MailKitOptions>(new MailKitOptions
        {
            Host = "smtp.invalid.test",
            PoolSize = 0
        });

        var act = () => new SmtpConnectionPool(options);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task RentAsync_WithUnreachableHost_ShouldThrow()
    {
        var options = new StaticOptionsMonitor<MailKitOptions>(new MailKitOptions
        {
            Host = "nonexistent.invalid.host.test",
            Port = 2525,
            UseSsl = false,
            PoolSize = 1
        });
        await using var pool = new SmtpConnectionPool(options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var act = async () => await pool.RentAsync(cts.Token);

        await act.Should().ThrowAsync<Exception>();
    }
}
