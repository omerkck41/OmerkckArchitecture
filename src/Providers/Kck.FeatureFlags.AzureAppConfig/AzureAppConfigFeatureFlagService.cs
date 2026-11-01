using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Kck.FeatureFlags.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.FeatureFlags.AzureAppConfig;

/// <summary>
/// <see cref="IFeatureFlagService"/> backed by Azure App Configuration.
/// Loads feature flags on first use and refreshes them on a configurable interval.
/// Supports both connection-string and Managed Identity authentication.
/// </summary>
public sealed partial class AzureAppConfigFeatureFlagService : IFeatureFlagService, IAsyncDisposable
{
    private readonly AzureAppConfigOptions _opts;
    private readonly ILogger<AzureAppConfigFeatureFlagService> _logger;
    private readonly ConfigurationClient _client;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, FeatureDefinition> _cache = new();
    // Raw JSON values stored separately for GetValueAsync (Parameters serialization).
    private readonly ConcurrentDictionary<string, string?> _rawValues = new();
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;
    // Written and read under _refreshLock only.
    private bool _loadedOnce;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    /// <summary>
    /// Initialises the service and builds the <see cref="ConfigurationClient"/>.
    /// </summary>
    public AzureAppConfigFeatureFlagService(
        IOptionsMonitor<AzureAppConfigOptions> options,
        ILogger<AzureAppConfigFeatureFlagService> logger)
        : this(options, logger, BuildClient(options.CurrentValue))
    {
    }

    /// <summary>
    /// Initialises the service with a caller-supplied <see cref="ConfigurationClient"/>, for example one built with
    /// custom credentials or a custom endpoint. <c>ConnectionString</c> and <c>Endpoint</c> are not used to build a
    /// client in this case.
    /// </summary>
    public AzureAppConfigFeatureFlagService(
        IOptionsMonitor<AzureAppConfigOptions> options,
        ILogger<AzureAppConfigFeatureFlagService> logger,
        ConfigurationClient client)
        : this(options, logger, client, TimeProvider.System)
    {
    }

    // Test seam: the refresh interval is measured on this clock.
    internal AzureAppConfigFeatureFlagService(
        IOptionsMonitor<AzureAppConfigOptions> options,
        ILogger<AzureAppConfigFeatureFlagService> logger,
        ConfigurationClient client,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _opts = options.CurrentValue;
        _logger = logger;
        _client = client;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default)
    {
        await EnsureFreshAsync(ct).ConfigureAwait(false);
        return _cache.TryGetValue(featureName, out var def) && def.Enabled;
    }

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string featureName, IFeatureContext context, CancellationToken ct = default)
        => await IsEnabledAsync(featureName, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> GetValueAsync<T>(string featureName, T defaultValue, CancellationToken ct = default)
    {
        await EnsureFreshAsync(ct).ConfigureAwait(false);

        if (!_rawValues.TryGetValue(featureName, out var raw) || raw is null)
            return defaultValue;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("parameters", out var paramsEl))
            {
                var result = JsonSerializer.Deserialize<T>(paramsEl.GetRawText());
                return result ?? defaultValue;
            }
            return defaultValue;
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeatureDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        await EnsureFreshAsync(ct).ConfigureAwait(false);
        return _cache.Values.ToList().AsReadOnly();
    }

    private async Task EnsureFreshAsync(CancellationToken ct)
    {
        if (_timeProvider.GetUtcNow() - _lastRefresh < _opts.RefreshInterval)
            return;

        await _refreshLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_timeProvider.GetUtcNow() - _lastRefresh < _opts.RefreshInterval)
                return;

            try
            {
                await RefreshAsync(ct).ConfigureAwait(false);
                _loadedOnce = true;
                _lastRefresh = _timeProvider.GetUtcNow();
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException
                                       && (ex is not OperationCanceledException || !ct.IsCancellationRequested))
            {
                // Any refresh failure counts as an outage: Azure.Core reports an unreachable store as an
                // AggregateException after its retries, a transport timeout as TaskCanceledException and a token failure
                // as AuthenticationFailedException. Only the caller's own cancellation propagates.
                // The first load has nothing to fall back on. Later failures keep the last snapshot (which may hold
                // zero flags) and back off for one interval, so an outage does not fail every caller (or hit the store
                // on every call).
                if (!_loadedOnce)
                    throw;

                LogRefreshFailedKeepingSnapshot(_logger, ex, _cache.Count);
                _lastRefresh = _timeProvider.GetUtcNow();
            }
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        var selector = new SettingSelector
        {
            KeyFilter = $"{_opts.KeyPrefix}*",
            LabelFilter = _opts.Label
        };

        var loaded = new Dictionary<string, FeatureDefinition>();
        var rawLoaded = new Dictionary<string, string?>();

        await foreach (var setting in _client.GetConfigurationSettingsAsync(selector, ct).ConfigureAwait(false))
        {
            var featureName = setting.Key.Replace(_opts.KeyPrefix, string.Empty, StringComparison.Ordinal);
            if (string.IsNullOrEmpty(featureName)) continue;

            bool enabled = false;
            FeatureFilterType filterType = FeatureFilterType.None;

            try
            {
                if (!string.IsNullOrEmpty(setting.Value))
                {
                    using var doc = JsonDocument.Parse(setting.Value);
                    if (doc.RootElement.TryGetProperty("enabled", out var enabledProp))
                        enabled = enabledProp.GetBoolean();
                }
            }
            catch (JsonException ex)
            {
                LogInvalidFlagJson(_logger, featureName, ex);
            }

            loaded[featureName] = new FeatureDefinition { Name = featureName, Enabled = enabled, FilterType = filterType };
            rawLoaded[featureName] = setting.Value;
        }

        foreach (var kvp in loaded)
            _cache[kvp.Key] = kvp.Value;
        foreach (var kvp in rawLoaded)
            _rawValues[kvp.Key] = kvp.Value;

        // Remove stale flags
        foreach (var key in _cache.Keys.Except(loaded.Keys).ToList())
        {
            _cache.TryRemove(key, out _);
            _rawValues.TryRemove(key, out _);
        }

        LogRefreshed(_logger, loaded.Count);
    }

    private static ConfigurationClient BuildClient(AzureAppConfigOptions opts)
    {
        if (!string.IsNullOrWhiteSpace(opts.ConnectionString))
            return new ConfigurationClient(opts.ConnectionString);

        if (!string.IsNullOrWhiteSpace(opts.Endpoint))
            return new ConfigurationClient(new Uri(opts.Endpoint), new DefaultAzureCredential());

        throw new InvalidOperationException(
            """
            [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions is invalid:
              Set either ConnectionString or Endpoint (for Managed Identity).
              Example: opt.ConnectionString = Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_CONNECTION");
            """);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _refreshLock.Dispose();
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Feature flag '{FeatureName}' has invalid JSON — skipping.")]
    private static partial void LogInvalidFlagJson(ILogger logger, string featureName, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Azure App Configuration refresh failed; keeping the last {Count} feature flags until the next refresh interval")]
    private static partial void LogRefreshFailedKeepingSnapshot(ILogger logger, Exception ex, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Azure App Configuration feature flags refreshed: {Count} flags loaded.")]
    private static partial void LogRefreshed(ILogger logger, int count);
}
