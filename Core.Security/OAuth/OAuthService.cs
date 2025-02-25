using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Core.Security.OAuth;

public class OAuthService
{
    private readonly OAuthConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OAuthService(OAuthConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration ?? throw new CustomException(nameof(configuration));
        _httpClient = httpClient ?? throw new CustomException(nameof(httpClient));
    }

    // Generates the authorization URL for the OAuth flow
    public string GetAuthorizationUrl()
    {
        string scope = string.Join(" ", _configuration.Scopes.Select(Uri.EscapeDataString));
        var url = $"{_configuration.AuthorizationEndpoint}?client_id={_configuration.ClientId}&redirect_uri={_configuration.RedirectUri}&response_type=code&scope={scope}";

        return url;
    }

    // Exchanges the authorization code for an access token
    public async Task<string> ExchangeCodeForTokenAsync(string authorizationCode)
    {
        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _configuration.ClientId),
            new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", _configuration.RedirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        });

        var response = await _httpClient.PostAsync(_configuration.TokenEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to exchange token: {response.StatusCode}. Error: {errorContent}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    // Refreshes the access token using a refresh token
    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
    {
        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _configuration.ClientId),
            new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        });

        var response = await _httpClient.PostAsync(_configuration.TokenEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to refresh token: {response.StatusCode}. Error: {errorContent}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    // Validates the token (e.g., checks expiration and signature)
    public async Task<bool> ValidateTokenAsync(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            // Validate token signature and expiration
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false, // Set to true if you want to validate the issuer
                ValidateAudience = false, // Set to true if you want to validate the audience
                ClockSkew = TimeSpan.Zero // No tolerance for expiration time
            };

            SecurityToken validatedToken;
            var principal = handler.ValidateToken(token, validationParameters, out validatedToken);

            // Additional custom validation logic can be added here
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }

    // Fetches user information using the access token
    public async Task<string> GetUserInfoAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _configuration.UserInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to fetch user info: {response.StatusCode}. Error: {errorContent}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    // Checks if the token is expired
    public bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.ValidTo < DateTime.UtcNow;
    }

    // Parses the token and returns the claims
    public IEnumerable<Claim> ParseTokenClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.Claims;
    }
}
