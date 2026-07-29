namespace AuthenticationApi.Application.Accounts;

/// <summary>
/// Permission read model.
/// </summary>
/// <param name="Id">The permission identifier.</param>
/// <param name="Name">The permission name.</param>
public sealed record PermissionResponse(int Id, string Name);
