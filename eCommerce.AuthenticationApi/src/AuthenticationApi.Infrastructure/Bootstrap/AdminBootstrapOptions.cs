namespace AuthenticationApi.Infrastructure.Bootstrap;

/// <summary>
/// Controls the opt-in creation of the first administrator across Keycloak, AuthenticationApi,
/// and UserApi. The password must come from a secret provider and must never be committed.
/// </summary>
public sealed class AdminBootstrapOptions
{
    public const string SectionName = "BootstrapAdmin";

    /// <summary>Gets whether startup may create an administrator when none has the Admin role.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets the administrator email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the administrator password supplied through secret configuration.</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>Gets the administrator profile first name.</summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Gets the administrator profile last name.</summary>
    public string LastName { get; init; } = string.Empty;
}
