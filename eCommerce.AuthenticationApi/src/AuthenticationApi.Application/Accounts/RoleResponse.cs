namespace AuthenticationApi.Application.Accounts;

/// <summary>
/// Role read model with its permissions.
/// </summary>
/// <param name="Id">The role identifier.</param>
/// <param name="Name">The role name.</param>
/// <param name="Permissions">Permissions assigned to the role.</param>
public sealed record RoleResponse(
    int Id,
    string Name,
    IReadOnlyCollection<PermissionResponse> Permissions);
