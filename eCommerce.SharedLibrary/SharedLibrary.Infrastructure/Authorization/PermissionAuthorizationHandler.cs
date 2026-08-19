using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SharedLibrary.Application.Authorization;

namespace SharedLibrary.Infrastructure.Authorization;

/// <summary>
/// Authorizes permission policies from application roles carried by the access token.
/// </summary>
/// <remarks>
/// Role and permission comparisons are case-insensitive. Unknown roles grant no permissions. Customer and seller
/// permissions use the local static map, while the administrator role receives every known application permission.
/// </remarks>
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
            [ApplicationRoles.Seller] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ApplicationPermissions.ProductRead,
                ApplicationPermissions.ProductCreateOwn,
                ApplicationPermissions.ProductUpdateOwn,
                ApplicationPermissions.ProductDeleteOwn,
                ApplicationPermissions.ProductReadOwn,
                ApplicationPermissions.ProductCreate,
                ApplicationPermissions.ImageUpload
            },
            [ApplicationRoles.Admin] = ApplicationPermissions.All.ToHashSet(StringComparer.OrdinalIgnoreCase)
        };

    /// <summary>Marks the requirement successful when any caller role grants the requested permission.</summary>
    /// <param name="context">The authorization context containing the caller claims.</param>
    /// <param name="requirement">The permission requirement being evaluated.</param>
    /// <returns>A completed task after the requirement is evaluated.</returns>
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
