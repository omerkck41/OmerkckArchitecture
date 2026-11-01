using AwesomeAssertions;
using Kck.Testing;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Kck.Security.TokenBlacklist.Redis.Tests;

/// <summary>
/// Connection handling: a shared <see cref="IConnectionMultiplexer"/> is used but never disposed; an own connection is
/// opened asynchronously on first use, once, and a failed connect is retried on the next call.
/// </summary>
public sealed class RedisTokenBlacklistConnectionTests
{
    private static StaticOptionsMonitor<RedisTokenBlacklistOptions> Options(string? connectionString = null) =>
        new(new RedisTokenBlacklistOptions
        {
            ConnectionString = connectionString!,
            KeyPrefix = "bl:",
            Database = 0
        });

    private static (IConnectionMultiplexer Connection, IDatabase Database) CreateConnection()
    {
        var connection = Substitute.For<IConnectionMultiplexer>();
        var db = Substitute.For<IDatabase>();
        connection.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        return (connection, db);
    }

    [Fact]
    public async Task RevokeAsync_WithSharedConnection_UsesItAndDoesNotDisposeIt()
    {
        var (connection, db) = CreateConnection();

        using (var sut = new RedisTokenBlacklistService(Options(), connection))
        {
            await sut.RevokeAsync("jti", TimeSpan.FromMinutes(15));
        }

        await db.Received(1).StringSetAsync(new RedisKey("bl:jti"), new RedisValue("revoked"), TimeSpan.FromMinutes(15));
        connection.DidNotReceive().Dispose();
    }

    [Fact]
    public async Task IsRevokedAsync_WithSharedConnection_ReadsKeyExists()
    {
        var (connection, db) = CreateConnection();
        db.KeyExistsAsync(new RedisKey("bl:jti"), Arg.Any<CommandFlags>()).Returns(true);
        using var sut = new RedisTokenBlacklistService(Options(), connection);

        var result = await sut.IsRevokedAsync("jti");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task OwnConnection_FirstConnectFails_SecondCallRetries()
    {
        var (connection, _) = CreateConnection();
        var calls = 0;
        Task<IConnectionMultiplexer> Connect(string cs)
        {
            calls++;
            return calls == 1
                ? Task.FromException<IConnectionMultiplexer>(new InvalidOperationException("redis down"))
                : Task.FromResult(connection);
        }

        using var sut = new RedisTokenBlacklistService(Options("localhost:6379"), Connect);

        var first = () => sut.RevokeAsync("jti", TimeSpan.FromMinutes(15));
        await first.Should().ThrowAsync<InvalidOperationException>().WithMessage("redis down");

        var second = () => sut.RevokeAsync("jti", TimeSpan.FromMinutes(15));
        await second.Should().NotThrowAsync();

        calls.Should().Be(2);
    }

    [Fact]
    public async Task OwnConnection_ConcurrentFirstUse_ConnectsOnce()
    {
        var (connection, _) = CreateConnection();
        var calls = 0;
        var connectStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<IConnectionMultiplexer> Connect(string cs)
        {
            Interlocked.Increment(ref calls);
            connectStarted.TrySetResult();
            await release.Task;
            return connection;
        }

        using var sut = new RedisTokenBlacklistService(Options("localhost:6379"), Connect);

        var checks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() => sut.IsRevokedAsync($"jti-{i}")))
            .ToArray();
        await connectStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Task.Delay(100); // let the other callers queue behind the first connect
        release.SetResult();
        await Task.WhenAll(checks).WaitAsync(TimeSpan.FromSeconds(10));

        Volatile.Read(ref calls).Should().Be(1);
    }

    [Fact]
    public async Task OwnConnection_ConcurrentCallersDuringFailedConnect_ShareOneAttempt()
    {
        var (connection, _) = CreateConnection();
        var calls = 0;
        var connectStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var gate = new TaskCompletionSource<IConnectionMultiplexer>(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<IConnectionMultiplexer> Connect(string cs)
        {
            // The first attempt waits on the gate; any later attempt connects at once.
            if (Interlocked.Increment(ref calls) > 1)
                return connection;

            connectStarted.TrySetResult();
            return await gate.Task;
        }

        using var sut = new RedisTokenBlacklistService(Options("localhost:6379"), Connect);

        var checks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() => sut.IsRevokedAsync($"jti-{i}")))
            .ToArray();
        await connectStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Task.Delay(100); // let the other callers join the in-flight attempt
        gate.SetException(new InvalidOperationException("redis down"));

        foreach (var check in checks)
        {
            await FluentActions.Awaiting(() => check.WaitAsync(TimeSpan.FromSeconds(10)))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage("redis down");
        }

        Volatile.Read(ref calls).Should().Be(1, "every concurrent caller awaited the same failed attempt");

        var later = () => sut.IsRevokedAsync("jti-later");
        await later.Should().NotThrowAsync();
        Volatile.Read(ref calls).Should().Be(2, "a failed attempt is not cached: the next call connects again");
    }

    [Fact]
    public async Task OwnConnection_AfterDispose_ThrowsObjectDisposedException()
    {
        var (connection, _) = CreateConnection();
        // Disposed explicitly in the middle of the test; the using guarantees it on an exception path too.
        using var sut = new RedisTokenBlacklistService(Options("localhost:6379"), _ => Task.FromResult(connection));
        await sut.IsRevokedAsync("jti");

        sut.Dispose();

        var act = () => sut.IsRevokedAsync("jti");
        await act.Should().ThrowAsync<ObjectDisposedException>();
        connection.Received(1).Dispose();
    }

    [Fact]
    public async Task OwnConnection_ConnectCompletesAfterDispose_DisposesTheNewConnection()
    {
        var (connection, _) = CreateConnection();
        var connectStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var gate = new TaskCompletionSource<IConnectionMultiplexer>(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<IConnectionMultiplexer> Connect(string cs)
        {
            connectStarted.TrySetResult();
            return await gate.Task;
        }

        // Disposed explicitly while the connect is in flight; the using guarantees it on an exception path too.
        using var sut = new RedisTokenBlacklistService(Options("localhost:6379"), Connect);
        var check = Task.Run(() => sut.IsRevokedAsync("jti"));
        await connectStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

        sut.Dispose();
        gate.SetResult(connection);

        var act = () => check.WaitAsync(TimeSpan.FromSeconds(10));
        await act.Should().ThrowAsync<ObjectDisposedException>();
        connection.Received(1).Dispose();
    }
}
