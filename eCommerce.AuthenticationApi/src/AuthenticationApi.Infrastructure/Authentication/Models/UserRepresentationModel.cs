namespace AuthenticationApi.Infrastructure.Authentication.Models;

/// <summary>
/// Keycloak user representation used by the admin API.
/// </summary>
public sealed class UserRepresentationModel
{
    public string Id { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public bool Enabled { get; init; }

    public bool EmailVerified { get; init; }

    public long CreatedTimestamp { get; init; }

    public IReadOnlyCollection<CredentialRepresentationModel> Credentials { get; init; } = [];

    public IReadOnlyCollection<string> RequiredActions { get; init; } = [];
}
