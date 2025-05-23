﻿using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Security.OAuth;

public class OAuthConfiguration
{
    // Immutable properties for security and thread safety
    public string ClientId { get; init; }
    public string ClientSecret { get; init; }
    public string AuthorizationEndpoint { get; init; }
    public string TokenEndpoint { get; init; }
    public string RedirectUri { get; init; }
    public string UserInfoEndpoint { get; init; } // Yeni eklenen özellik
    public string[] Scopes { get; init; } = Array.Empty<string>();

    // Constructor for initializing required properties
    public OAuthConfiguration(OAuthSettings settings)
    {
        if (settings == null)
            throw new CustomArgumentException(nameof(settings));

        if (string.IsNullOrEmpty(settings.ClientId))
            throw new CustomArgumentException(nameof(settings.ClientId));
        if (string.IsNullOrEmpty(settings.ClientSecret))
            throw new CustomArgumentException(nameof(settings.ClientSecret));
        if (string.IsNullOrEmpty(settings.AuthorizationEndpoint))
            throw new CustomArgumentException(nameof(settings.AuthorizationEndpoint));
        if (string.IsNullOrEmpty(settings.TokenEndpoint))
            throw new CustomArgumentException(nameof(settings.TokenEndpoint));
        if (string.IsNullOrEmpty(settings.RedirectUri))
            throw new CustomArgumentException(nameof(settings.RedirectUri));
        if (string.IsNullOrEmpty(settings.UserInfoEndpoint))
            throw new CustomArgumentException(nameof(settings.UserInfoEndpoint));

        ClientId = settings.ClientId;
        ClientSecret = settings.ClientSecret;
        AuthorizationEndpoint = settings.AuthorizationEndpoint;
        TokenEndpoint = settings.TokenEndpoint;
        RedirectUri = settings.RedirectUri;
        UserInfoEndpoint = settings.UserInfoEndpoint;
        Scopes = settings.Scopes ?? Array.Empty<string>();
    }
}