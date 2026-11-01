using Azure;
using Azure.Data.AppConfiguration;
using AwesomeAssertions;
using Kck.FeatureFlags.Abstractions;
using Kck.FeatureFlags.AzureAppConfig;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Kck.FeatureFlags.AzureAppConfig.Tests;

/// <summary>
/// Refresh behaviour: the clock is a manual <see cref="TimeProvider"/>, so the tests advance time past the refresh
/// interval instead of waiting for it.
/// </summary>
public sealed class AzureAppConfigFeatureFlagServiceRefreshTests
{
    // Not a credential: the store does not exist and the tests never reach the network with it.
    private const string FakeConnectionString = "Endpoint=https://kck-test.invalid;Id=a;Secret=Yg==";

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PastRefreshInterval = RefreshInterval + TimeSpan.FromSeconds(1);

    [Fact]
    public async Task IsEnabledAsync_WhenRefreshFailsAfterFirstLoad_KeepsLastSnapshot()
    {
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(BetaFlag()), FailingPageable());
        var time = new TestTimeProvider();
        await using var sut = CreateService(client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), time);

        (await sut.IsEnabledAsync("beta")).Should().BeTrue();
        time.Advance(PastRefreshInterval);

        (await sut.IsEnabledAsync("beta")).Should().BeTrue("the last loaded flags are kept while the store is down");
        (await sut.GetAllAsync()).Should().ContainSingle().Which.Name.Should().Be("beta");
        (await sut.IsEnabledAsync("beta")).Should().BeTrue();

        client.Received(2).GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsEnabledAsync_WithinRefreshInterval_DoesNotQueryTheStoreAgain()
    {
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(BetaFlag()));
        var time = new TestTimeProvider();
        await using var sut = CreateService(client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), time);

        await sut.IsEnabledAsync("beta");
        time.Advance(RefreshInterval - TimeSpan.FromSeconds(1));
        await sut.IsEnabledAsync("beta");

        client.Received(1).GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsEnabledAsync_AfterFailedRefresh_RetriesAfterTheNextInterval()
    {
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(BetaFlag()), FailingPageable(), Pageable(BetaFlag(enabled: false)));
        var time = new TestTimeProvider();
        await using var sut = CreateService(client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), time);

        await sut.IsEnabledAsync("beta");
        time.Advance(PastRefreshInterval);
        (await sut.IsEnabledAsync("beta")).Should().BeTrue("the failed refresh keeps the snapshot");
        time.Advance(PastRefreshInterval);

        (await sut.IsEnabledAsync("beta")).Should().BeFalse("the store is back and the flag was switched off");
        client.Received(3).GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsEnabledAsync_WhenRefreshThrowsAggregateException_KeepsLastSnapshot()
    {
        // Azure.Core's retry policy wraps the per-try failures when every try failed (unreachable store).
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(
                Pageable(BetaFlag()),
                ThrowingPageable(new AggregateException(
                    new RequestFailedException("x"), new RequestFailedException("y"))));
        var time = new TestTimeProvider();
        var logger = new CapturingLogger<AzureAppConfigFeatureFlagService>();
        await using var sut = CreateService(client, logger, time);

        (await sut.IsEnabledAsync("beta")).Should().BeTrue();
        time.Advance(PastRefreshInterval);

        (await sut.IsEnabledAsync("beta")).Should().BeTrue("the last loaded flags are kept while the store is down");
        logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning)
            .Which.Exception.Should().BeOfType<AggregateException>();
    }

    [Fact]
    public async Task IsEnabledAsync_WhenRefreshTimesOut_KeepsLastSnapshot()
    {
        // A transport timeout surfaces as TaskCanceledException although the caller did not cancel.
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(BetaFlag()), ThrowingPageable(new TaskCanceledException("timeout")));
        var time = new TestTimeProvider();
        await using var sut = CreateService(client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), time);

        (await sut.IsEnabledAsync("beta")).Should().BeTrue();
        time.Advance(PastRefreshInterval);

        (await sut.IsEnabledAsync("beta")).Should().BeTrue("a timeout the caller did not ask for keeps the snapshot");
    }

    [Fact]
    public async Task IsEnabledAsync_WhenRefreshFailsOnEmptyStore_DoesNotThrow()
    {
        // The first load succeeded with zero flags: that is a valid snapshot, not "nothing loaded yet".
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(), FailingPageable());
        var time = new TestTimeProvider();
        await using var sut = CreateService(client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), time);

        (await sut.IsEnabledAsync("beta")).Should().BeFalse();
        time.Advance(PastRefreshInterval);

        var enabled = await sut.IsEnabledAsync("beta");

        enabled.Should().BeFalse();
        client.Received(2).GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsEnabledAsync_WhenCallerCancels_Propagates()
    {
        using var cts = new CancellationTokenSource();
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(BetaFlag()), CancellingPageable(cts));
        var time = new TestTimeProvider();
        await using var sut = CreateService(client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), time);

        await sut.IsEnabledAsync("beta");
        time.Advance(PastRefreshInterval);

        // The caller's token is cancelled while the refresh runs: that cancellation is not swallowed as an outage.
        var act = () => sut.IsEnabledAsync("beta", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IsEnabledAsync_WhenFirstLoadFails_Throws()
    {
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(FailingPageable());
        await using var sut = CreateService(
            client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), new TestTimeProvider());

        var act = () => sut.IsEnabledAsync("beta");

        (await act.Should().ThrowAsync<RequestFailedException>()).Which.Status.Should().Be(503);
    }

    [Fact]
    public async Task IsEnabledAsync_WhenFirstLoadThrowsAggregateException_Throws()
    {
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(ThrowingPageable(new AggregateException(new RequestFailedException("x"))));
        await using var sut = CreateService(
            client, new CapturingLogger<AzureAppConfigFeatureFlagService>(), new TestTimeProvider());

        var act = () => sut.IsEnabledAsync("beta");

        await act.Should().ThrowAsync<AggregateException>();
    }

    [Fact]
    public async Task WarningIsLogged_WhenRefreshFails()
    {
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(BetaFlag()), FailingPageable());
        var logger = new CapturingLogger<AzureAppConfigFeatureFlagService>();
        var time = new TestTimeProvider();
        await using var sut = CreateService(client, logger, time);

        await sut.IsEnabledAsync("beta");
        logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
        time.Advance(PastRefreshInterval);
        await sut.IsEnabledAsync("beta");

        var warning = logger.Entries.Should().ContainSingle(e => e.Level == LogLevel.Warning).Subject;
        warning.Message.Should().Contain("1");
        warning.Exception.Should().BeOfType<RequestFailedException>();
    }

    [Fact]
    public async Task AddKckFeatureFlagsAzureAppConfig_WithRegisteredClient_UsesIt()
    {
        var client = Substitute.For<ConfigurationClient>();
        client.GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>())
            .Returns(Pageable(BetaFlag()));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(client);
        services.AddKckFeatureFlagsAzureAppConfig(_ => { });
        await using var provider = services.BuildServiceProvider();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        var flags = provider.GetRequiredService<IFeatureFlagService>();

        (await flags.IsEnabledAsync("beta", cts.Token)).Should().BeTrue();
        client.Received(1).GetConfigurationSettingsAsync(Arg.Any<SettingSelector>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddKckFeatureFlagsAzureAppConfig_WithRegisteredClientAndEmptyOptions_PassesValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<ConfigurationClient>());
        services.AddKckFeatureFlagsAzureAppConfig(_ => { });
        await using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptionsMonitor<AzureAppConfigOptions>>().CurrentValue;

        act.Should().NotThrow();
    }

    [Fact]
    public async Task AddKckFeatureFlagsAzureAppConfig_WithoutClientAndEmptyOptions_FailsValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckFeatureFlagsAzureAppConfig(_ => { });
        await using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptionsMonitor<AzureAppConfigOptions>>().CurrentValue;

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public async Task AddKckFeatureFlagsAzureAppConfig_WithoutRegisteredClient_ResolvesService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckFeatureFlagsAzureAppConfig(o => o.ConnectionString = FakeConnectionString);
        await using var provider = services.BuildServiceProvider();

        var flags = provider.GetRequiredService<IFeatureFlagService>();

        flags.Should().BeOfType<AzureAppConfigFeatureFlagService>();
    }

    private static AzureAppConfigFeatureFlagService CreateService(
        ConfigurationClient client, ILogger<AzureAppConfigFeatureFlagService> logger, TimeProvider timeProvider) =>
        new(
            new StaticOptionsMonitor<AzureAppConfigOptions>(new AzureAppConfigOptions
            {
                RefreshInterval = RefreshInterval
            }),
            logger,
            client,
            timeProvider);

    private static ConfigurationSetting BetaFlag(bool enabled = true) =>
        new(".appconfig.featureflag/beta", enabled
            ? """{"id":"beta","enabled":true}"""
            : """{"id":"beta","enabled":false}""");

    private static AsyncPageable<ConfigurationSetting> Pageable(params ConfigurationSetting[] items) =>
        AsyncPageable<ConfigurationSetting>.FromPages(
            [Page<ConfigurationSetting>.FromValues(items, null, Substitute.For<Response>())]);

    private static AsyncPageable<ConfigurationSetting> FailingPageable() =>
        ThrowingPageable(new RequestFailedException(503, "unavailable"));

    // The exception surfaces while the pages are enumerated, as with the real client.
    private static AsyncPageable<ConfigurationSetting> ThrowingPageable(Exception exception) =>
        AsyncPageable<ConfigurationSetting>.FromPages(ThrowingPages(exception));

    private static IEnumerable<Page<ConfigurationSetting>> ThrowingPages(Exception exception)
    {
        yield return Throw(exception);
    }

    private static Page<ConfigurationSetting> Throw(Exception exception) => throw exception;

    // Simulates the caller cancelling while the request is in flight: the token is cancelled, then observed.
    private static AsyncPageable<ConfigurationSetting> CancellingPageable(CancellationTokenSource cts) =>
        AsyncPageable<ConfigurationSetting>.FromPages(CancellingPages(cts));

    private static IEnumerable<Page<ConfigurationSetting>> CancellingPages(CancellationTokenSource cts)
    {
        yield return CancelAndThrow(cts);
    }

    private static Page<ConfigurationSetting> CancelAndThrow(CancellationTokenSource cts)
    {
        cts.Cancel();
        throw new OperationCanceledException(cts.Token);
    }
}

/// <summary>Records every log entry with its level, formatted message and exception.</summary>
internal sealed class CapturingLogger<T> : ILogger<T>
{
    private readonly Lock _gate = new();
    private readonly List<(LogLevel Level, string Message, Exception? Exception)> _entries = [];

    public IReadOnlyList<(LogLevel Level, string Message, Exception? Exception)> Entries
    {
        get
        {
            lock (_gate)
                return [.. _entries];
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (_gate)
            _entries.Add((logLevel, formatter(state, exception), exception));
    }
}

/// <summary>Manually advanced <see cref="TimeProvider"/>: wall clock and timestamps move only on <see cref="Advance"/>.</summary>
internal sealed class TestTimeProvider : TimeProvider
{
    private long _ticks = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).UtcTicks;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override long GetTimestamp() => Interlocked.Read(ref _ticks);

    public override DateTimeOffset GetUtcNow() => new(Interlocked.Read(ref _ticks), TimeSpan.Zero);

    public void Advance(TimeSpan by) => Interlocked.Add(ref _ticks, by.Ticks);
}
