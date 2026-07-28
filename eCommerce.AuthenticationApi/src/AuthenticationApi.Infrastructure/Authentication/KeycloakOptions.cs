namespace AuthenticationApi.Infrastructure.Authentication;

/// <summary>
/// Keycloak endpoints and client credentials.
/// </summary>
public sealed class KeycloakOptions
{
    public string AdminUrl { get; init; } = string.Empty;

    public string TokenUrl { get; init; } = string.Empty;

    public string AdminClientId { get; init; } = string.Empty;

    public string AdminClientSecret { get; init; } = string.Empty;

    public string AuthClientId { get; init; } = string.Empty;

    public string AuthClientSecret { get; init; } = string.Empty;
}
