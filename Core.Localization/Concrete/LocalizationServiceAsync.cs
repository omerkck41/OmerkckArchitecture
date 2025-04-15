using Core.Localization.Abstract;
using Core.Localization.Cache;
using Core.Localization.Models;
using Core.Localization.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Core.Localization.Concrete;

/// <summary>
/// Yerelleştirme servisi asenkron implementasyonu.
/// </summary>
public class LocalizationServiceAsync : ILocalizationServiceAsync
{
    private readonly LocalizationSourceManagerAsync _sourceManager;
    private readonly IOptions<LocalizationOptions> _options;
    private readonly ILogger<LocalizationServiceAsync> _logger;
    private CultureInfo _currentCulture;
    private readonly List<CultureInfo> _supportedCultures;
    private readonly IDistributedCacheManagerAsync _cacheManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationServiceAsync"/> class.
    /// </summary>
    /// <param name="sources">The localization sources.</param>
    /// <param name="options">The localization options.</param>
    /// <param name="logger">The logger instance.</param>
    public LocalizationServiceAsync(
         LocalizationSourceManagerAsync sourceManager,
         IOptions<LocalizationOptions> options,
         ILogger<LocalizationServiceAsync> logger,
         IDistributedCacheManagerAsync cacheManager)
    {
        _sourceManager = sourceManager;
        _options = options;
        _logger = logger;
        _cacheManager = cacheManager;

        // Desteklenen kültürleri yükle
        _supportedCultures = options.Value.SupportedCultures
            .Select(c => new CultureInfo(c))
            .ToList();

        // Varsayılan kültürü ayarla
        _currentCulture = new CultureInfo(options.Value.DefaultCulture);

        _logger.LogInformation("Localization service initialized with default culture: {Culture}", _currentCulture.Name);
    }

    /// <inheritdoc />
    public async Task<string> GetStringAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Empty localization key requested");
            return string.Empty;
        }

        // Kaynak yöneticisinden çevirileri al (önbelleği de yönetir)
        var translations = await _sourceManager.LoadTranslationsForCultureAsync(_currentCulture);

        if (translations.TryGetValue(key, out var translation) && !string.IsNullOrEmpty(translation))
        {
            return translation;
        }

        // Çeviri bulunamazsa, ayarlara göre key veya boş döndür.
        _logger.LogDebug("Translation not found for key: {Key} in culture: {Culture}", key, _currentCulture.Name);
        return _options.Value.ReturnKeyIfNotFound ? key : string.Empty;
    }

    /// <inheritdoc />
    public async Task<string> GetStringAsync(string key, params object[] args)
    {
        var format = await GetStringAsync(key);

        if (string.IsNullOrEmpty(format) || args.Length == 0)
        {
            return format;
        }

        try
        {
            // CPU-bound formatlama işlemi olduğu için doğrudan string.Format kullanıyoruz.
            var formatted = string.Format(_currentCulture, format, args);
            return formatted;
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Failed to format localized string with key: {Key}", key);
            return format;
        }
    }

    /// <inheritdoc />
    public CultureInfo GetCurrentCulture() => _currentCulture;

    /// <inheritdoc />
    public void SetCurrentCulture(CultureInfo culture)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (!_supportedCultures.Any(c => c.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Unsupported culture requested: {Culture}", culture.Name);
            throw new ArgumentException($"Culture '{culture.Name}' is not in the list of supported cultures.");
        }

        _currentCulture = culture;
        _logger.LogDebug("Current culture changed to: {Culture}", _currentCulture.Name);

        // Thread kültürü de güncellenir.
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    /// <inheritdoc />
    public void SetCurrentCulture(string cultureName)
    {
        if (string.IsNullOrEmpty(cultureName))
        {
            throw new ArgumentNullException(nameof(cultureName));
        }

        try
        {
            var culture = new CultureInfo(cultureName);
            SetCurrentCulture(culture);
        }
        catch (CultureNotFoundException ex)
        {
            _logger.LogError(ex, "Invalid culture name: {CultureName}", cultureName);
            throw;
        }
    }

    /// <inheritdoc />
    public IEnumerable<CultureInfo> GetSupportedCultures() => _supportedCultures;
}