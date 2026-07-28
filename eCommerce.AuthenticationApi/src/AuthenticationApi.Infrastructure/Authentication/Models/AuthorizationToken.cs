using System.Text.Json.Serialization;

namespace AuthenticationApi.Infrastructure.Authentication.Models;

/// <summary>
/// Token response returned by Keycloak.
/// </summary>
public sealed class AuthorizationToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
}
