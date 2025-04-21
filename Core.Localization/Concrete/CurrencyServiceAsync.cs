using Core.Localization.Abstract;
using Core.Localization.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace Core.Localization.Concrete;

/// <summary>
/// Para birimi servisi implementasyonu (asenkron versiyon)
/// </summary>
public class CurrencyServiceAsync : ICurrencyServiceAsync
{
    private readonly ILocalizationServiceAsync _localizationService;
    private readonly IOptionsMonitor<CurrencyOptions> _currencyOptions;
    private readonly ILogger<CurrencyServiceAsync> _logger;
    private ExchangeRateModel? _exchangeRates;
    private readonly Dictionary<string, string> _currencySymbols = new()
        {
            { "TRY", "₺" },
            { "USD", "$" },
            { "EUR", "€" },
            { "GBP", "£" },
            { "JPY", "¥" },
            // Diğer para birimleri eklenebilir
        };

    public CurrencyServiceAsync(
        ILocalizationServiceAsync localizationService,
        IOptionsMonitor<CurrencyOptions> currencyOptions,
        ILogger<CurrencyServiceAsync> logger)
    {
        _localizationService = localizationService;
        _currencyOptions = currencyOptions;
        _logger = logger;
    }

    /// <summary>
    /// Mevcut kültüre göre asenkron para formatlama.
    /// CPU-bound işlemler olduğundan Task.FromResult kullanılarak async hale getirilir.
    /// </summary>
    public async Task<string> FormatCurrencyAsync(decimal amount)
    {
        return await Task.FromResult(FormatCurrency(amount, _localizationService.GetCurrentCulture()));
    }

    /// <summary>
    /// Belirtilen kültüre göre asenkron para formatlama.
    /// </summary>
    public async Task<string> FormatCurrencyAsync(decimal amount, CultureInfo culture)
    {
        return await Task.FromResult(FormatCurrency(amount, culture));
    }

    /// <summary>
    /// Senkron para formatlama metodu (iç kullanım için)
    /// </summary>
    private string FormatCurrency(decimal amount, CultureInfo culture)
    {
        // Kültür bilgisinin kopyasını oluştur
        var cultureForFormat = (CultureInfo)culture.Clone();

        // Para birimi sembolünü belirle
        string currencySymbol = GetCurrencySymbolForCulture(culture);

        // NumberFormatInfo üzerinde değişiklik yap
        cultureForFormat.NumberFormat.CurrencySymbol = currencySymbol;
        cultureForFormat.NumberFormat.CurrencyPositivePattern = GetCurrencyPositivePatternForCulture(culture);

        // Formatla ve döndür
        return amount.ToString("C", cultureForFormat);
    }

    /// <inheritdoc />
    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        // Para birimleri aynı ise dönüştürmeye gerek yok
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return amount;
        }

        // Döviz kurlarını yükle veya güncelle
        if (_exchangeRates == null || IsExchangeRatesExpired())
        {
            await UpdateExchangeRatesAsync();
        }

        if (_exchangeRates == null)
        {
            _logger.LogError("Exchange rates are not available");
            throw new InvalidOperationException("Exchange rates are not available");
        }

        // Kaynak para birimi döviz kuru taban para birimi değilse, önce taban para birimine dönüştürülür
        decimal convertedAmount = amount;

        if (!_exchangeRates.BaseCurrency.Equals(fromCurrency, StringComparison.OrdinalIgnoreCase))
        {
            if (!_exchangeRates.Rates.TryGetValue(fromCurrency, out var fromRate) || fromRate == 0)
            {
                _logger.LogError("Exchange rate not found for currency: {Currency}", fromCurrency);
                throw new KeyNotFoundException($"Exchange rate not found for currency: {fromCurrency}");
            }

            // Taban para birimine dönüştür
            convertedAmount = amount / fromRate;
        }

        // Hedef para birimi taban para birimi değilse oran ile çarp
        if (!_exchangeRates.BaseCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            if (!_exchangeRates.Rates.TryGetValue(toCurrency, out var toRate))
            {
                _logger.LogError("Exchange rate not found for currency: {Currency}", toCurrency);
                throw new KeyNotFoundException($"Exchange rate not found for currency: {toCurrency}");
            }

            convertedAmount *= toRate;
        }

        return Math.Round(convertedAmount, 4);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetSupportedCurrencies()
    {
        var currencies = new List<string>();

        if (_exchangeRates != null)
        {
            currencies.Add(_exchangeRates.BaseCurrency);
            currencies.AddRange(_exchangeRates.Rates.Keys);
        }
        else
        {
            currencies.AddRange(_currencySymbols.Keys);
        }

        return currencies.Distinct().OrderBy(c => c);
    }

    private string GetCurrencySymbolForCulture(CultureInfo culture)
    {
        // RegionInfo'dan para birimi kodunu al
        var region = new RegionInfo(culture.Name);
        var currencyCode = region.ISOCurrencySymbol;

        // Para birimi sembolü varsa kullan, yoksa default değeri kullan
        if (_currencySymbols.TryGetValue(currencyCode, out var symbol))
        {
            return symbol;
        }

        return region.CurrencySymbol;
    }

    private int GetCurrencyPositivePatternForCulture(CultureInfo culture)
    {
        // Kültüre göre para birimi formatını belirle
        // 0: $n (sembol önde)
        // 1: n$ (sembol arkada)
        // 2: $ n (sembol önde boşluklu)
        // 3: n $ (sembol arkada boşluklu)

        var region = new RegionInfo(culture.Name);

        if (culture.TextInfo.IsRightToLeft)
        {
            return 1; // n$
        }

        if (region.Name.StartsWith("FR") || region.Name.StartsWith("DE") ||
            region.Name.StartsWith("IT") || region.Name.StartsWith("ES"))
        {
            return 3; // n $
        }

        if (region.Name.Equals("TR", StringComparison.OrdinalIgnoreCase))
        {
            return 1; // n$
        }

        return 0; // $n
    }

    private bool IsExchangeRatesExpired()
    {
        if (_exchangeRates == null)
        {
            return true;
        }

        var updateInterval = TimeSpan.FromMinutes(_currencyOptions.CurrentValue.UpdateInterval);
        return DateTime.UtcNow - _exchangeRates.LastUpdated > updateInterval;
    }

    private async Task UpdateExchangeRatesAsync()
    {
        try
        {
            var apiUrl = _currencyOptions.CurrentValue.ExchangeRateApiUrl;
            var apiKey = _currencyOptions.CurrentValue.ExchangeRateApiKey;

            if (string.IsNullOrEmpty(apiUrl))
            {
                _exchangeRates = CreateDemoExchangeRates();
                return;
            }

            using var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(apiKey))
            {
                httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
            }

            var response = await httpClient.GetStringAsync(apiUrl);
            _exchangeRates = ParseExchangeRateResponse(response);

            _logger.LogInformation("Exchange rates updated successfully. Base currency: {BaseCurrency}", _exchangeRates.BaseCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update exchange rates");
            _exchangeRates = CreateDemoExchangeRates();
        }
    }

    private ExchangeRateModel ParseExchangeRateResponse(string response)
    {
        try
        {
            var responseObj = JsonSerializer.Deserialize<JsonElement>(response);

            var model = new ExchangeRateModel
            {
                BaseCurrency = responseObj.GetProperty("base").GetString() ?? "USD",
                LastUpdated = DateTime.UtcNow
            };

            var rates = responseObj.GetProperty("rates");
            var ratesDictionary = new Dictionary<string, decimal>();

            foreach (var rate in rates.EnumerateObject())
            {
                ratesDictionary[rate.Name] = rate.Value.GetDecimal();
            }

            model.Rates = ratesDictionary;
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse exchange rate response");
            throw;
        }
    }

    private ExchangeRateModel CreateDemoExchangeRates()
    {
        return new ExchangeRateModel
        {
            BaseCurrency = "USD",
            LastUpdated = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.92m },
                    { "TRY", 32.45m },
                    { "GBP", 0.78m },
                    { "JPY", 149.50m },
                    { "CNY", 7.30m },
                    { "RUB", 97.25m },
                    { "INR", 83.15m },
                    { "AUD", 1.55m },
                    { "CAD", 1.37m },
                    { "CHF", 0.91m }
                }
        };
    }
}