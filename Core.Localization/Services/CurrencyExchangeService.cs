using Polly;
using Polly.Retry;

namespace Core.Localization.Services;

/// <summary>
/// Service for fetching currency exchange rates with retry policy.
/// </summary>
public class CurrencyExchangeService
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy _retryPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyExchangeService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to be used for making requests.</param>
    public CurrencyExchangeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    /// <summary>
    /// Gets the exchange rates from the specified API URL.
    /// </summary>
    /// <param name="apiUrl">The API URL to fetch exchange rates from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the exchange rates as a string.</returns>
    public async Task<string> GetExchangeRatesAsync(string apiUrl)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.GetStringAsync(apiUrl);
        });
    }
}