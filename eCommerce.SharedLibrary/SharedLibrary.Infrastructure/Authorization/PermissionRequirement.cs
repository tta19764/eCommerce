using Microsoft.AspNetCore.Authorization;

namespace SharedLibrary.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for one application permission.
/// </summary>
/// <param name="Permission">The permission name required by the endpoint.</param>
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
