namespace Core.Security.OAuth;

public class OAuthService
{
    private readonly OAuthConfiguration _configuration;

    public OAuthService(OAuthConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetAuthorizationUrl()
    {
        string scope = string.Join("%20", _configuration.Scopes);
        var url = $"{_configuration.AuthorizationEndpoint}?client_id={_configuration.ClientId}&redirect_uri={_configuration.RedirectUri}&response_type=code&scope={scope}";

        return url;
    }

    public async Task<string> ExchangeCodeForTokenAsync(string authorizationCode)
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
            throw new HttpRequestException($"Failed to exchange token: {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
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
            throw new HttpRequestException($"Failed to refresh token: {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}
