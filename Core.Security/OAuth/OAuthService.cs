using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using SecurityTokenException = Core.CrossCuttingConcerns.GlobalException.Exceptions.SecurityTokenException;

namespace Core.Security.OAuth;

public class OAuthService : IOAuthService
{
    private readonly OAuthConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OAuthService(OAuthConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration ?? throw new CustomArgumentException(nameof(configuration));
        _httpClient = httpClient ?? throw new CustomArgumentException(nameof(httpClient));
    }

    public string GetAuthorizationUrl()
    {
        var scopes = string.Join(" ", _configuration.Scopes.Select(Uri.EscapeDataString));
        return $"{_configuration.AuthorizationEndpoint}?client_id={_configuration.ClientId}&redirect_uri={_configuration.RedirectUri}&response_type=code&scope={scopes}";
    }

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
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

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
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            handler.ValidateToken(token, validationParameters, out _);
            return Task.FromResult(true);
        }
        catch (SecurityTokenException)
        {
            return Task.FromResult(false);
        }
    }

    public async Task<string> GetUserInfoAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _configuration.UserInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.ValidTo < DateTime.UtcNow;
    }

    public IEnumerable<Claim> ParseTokenClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.Claims;
    }
}
