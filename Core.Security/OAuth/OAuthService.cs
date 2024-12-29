using Microsoft.Extensions.Logging;

namespace Core.Security.OAuth;

public class OAuthService
{
    private readonly OAuthConfiguration _configuration;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(OAuthConfiguration configuration, ILogger<OAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GetAuthorizationUrl()
    {
        try
        {
            string scope = string.Join("%20", _configuration.Scopes);
            var url = $"{_configuration.AuthorizationEndpoint}?client_id={_configuration.ClientId}&redirect_uri={_configuration.RedirectUri}&response_type=code&scope={scope}";
            _logger.LogInformation("Generated Authorization URL: {Url}", url);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate authorization URL.");
            throw;
        }
    }

    public async Task<string> ExchangeCodeForTokenAsync(string authorizationCode)
    {
        try
        {
            using var httpClient = new HttpClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuration.ClientId),
                new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", _configuration.RedirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await httpClient.PostAsync(_configuration.TokenEndpoint, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed: {Error}", errorContent);
                throw new HttpRequestException($"Failed to exchange token: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully exchanged authorization code for token.");
            return responseBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange authorization code for token.");
            throw;
        }
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuration.ClientId),
                new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            });

            var response = await httpClient.PostAsync(_configuration.TokenEndpoint, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token refresh failed: {Error}", errorContent);
                throw new HttpRequestException($"Failed to refresh token: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully refreshed access token.");
            return responseBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh access token.");
            throw;
        }
    }
}
