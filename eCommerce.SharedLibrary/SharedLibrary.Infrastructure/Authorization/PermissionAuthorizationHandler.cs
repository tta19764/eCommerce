using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SharedLibrary.Application.Authorization;

namespace SharedLibrary.Infrastructure.Authorization;

/// <summary>
/// Authorizes permission policies from application roles carried by the access token.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> RolePermissions =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [ApplicationRoles.Customer] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ApplicationPermissions.ProductRead,
                ApplicationPermissions.OrderCreate,
                ApplicationPermissions.ImageUpload
            },
            [ApplicationRoles.Admin] = ApplicationPermissions.All.ToHashSet(StringComparer.OrdinalIgnoreCase)
        };

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Keycloak roles stay coarse-grained. This local map expands them into the fine-grained
        // endpoint permissions used by the services without requiring every permission as a token claim.
        if (context.User.Claims
            .Where(IsRoleClaim)
            .Select(claim => claim.Value)
            .Any(role => RoleHasPermission(role, requirement.Permission)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool RoleHasPermission(string role, string permission)
    {
        return RolePermissions.TryGetValue(role, out var permissions) &&
            permissions.Contains(permission);
    }

    private static bool IsRoleClaim(Claim claim)
    {
        return claim.Type is ClaimTypes.Role or "role" or "roles";
    }
}
