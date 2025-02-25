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
    public OAuthConfiguration(
        string clientId,
        string clientSecret,
        string authorizationEndpoint,
        string tokenEndpoint,
        string redirectUri,
        string userInfoEndpoint, // Yeni eklenen parametre
        string[] scopes)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new CustomException(nameof(clientId));
        if (string.IsNullOrEmpty(clientSecret))
            throw new CustomException(nameof(clientSecret));
        if (string.IsNullOrEmpty(authorizationEndpoint))
            throw new CustomException(nameof(authorizationEndpoint));
        if (string.IsNullOrEmpty(tokenEndpoint))
            throw new CustomException(nameof(tokenEndpoint));
        if (string.IsNullOrEmpty(redirectUri))
            throw new CustomException(nameof(redirectUri));
        if (string.IsNullOrEmpty(userInfoEndpoint)) // Yeni eklenen kontrol
            throw new CustomException(nameof(userInfoEndpoint));

        ClientId = clientId;
        ClientSecret = clientSecret;
        AuthorizationEndpoint = authorizationEndpoint;
        TokenEndpoint = tokenEndpoint;
        RedirectUri = redirectUri;
        UserInfoEndpoint = userInfoEndpoint; // Yeni eklenen özellik ataması
        Scopes = scopes ?? Array.Empty<string>();
    }
}