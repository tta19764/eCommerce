namespace AuthenticationApi.Infrastructure.Authentication.Models;

/// <summary>
/// Keycloak credential representation used when creating password credentials.
/// </summary>
public sealed class CredentialRepresentationModel
{
    public bool Temporary { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
