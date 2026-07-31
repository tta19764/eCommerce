using System.Text.Json.Serialization;

namespace AuthenticationApi.Infrastructure.Authentication.Models;

/// <summary>
/// Keycloak role representation used by the admin API.
/// </summary>
public sealed class RoleRepresentationModel
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
