namespace Core.Security.OAuth;

public class OAuthSettings
{
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string AuthorizationEndpoint { get; set; } = default!;
    public string TokenEndpoint { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string UserInfoEndpoint { get; set; } = default!;
    public string[] Scopes { get; set; } = Array.Empty<string>();
}