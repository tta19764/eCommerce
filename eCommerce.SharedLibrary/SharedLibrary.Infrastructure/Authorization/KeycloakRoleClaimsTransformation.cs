using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace SharedLibrary.Infrastructure.Authorization;

/// <summary>
/// Converts Keycloak realm and client role payloads into standard ASP.NET role claims.
/// </summary>
public sealed class KeycloakRoleClaimsTransformation : IClaimsTransformation
{
    /// <summary>
    /// Executes the TransformAsync operation.
    /// </summary>
    /// <param name="principal">The principal value.</param>
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(principal);
        }

        var existingRoles = principal.Claims
            .Where(IsRoleClaim)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var keycloakRoles = ExtractKeycloakRoles(principal).ToArray();

        foreach (var role in keycloakRoles)
        {
            if (existingRoles.Add(role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        return Task.FromResult(principal);
    }

    private static IEnumerable<string> ExtractKeycloakRoles(ClaimsPrincipal principal)
    {
        foreach (var role in ExtractRealmRoles(principal))
        {
            yield return role;
        }

        foreach (var role in ExtractResourceRoles(principal))
        {
            yield return role;
        }
    }

    private static IEnumerable<string> ExtractRealmRoles(ClaimsPrincipal principal)
    {
        foreach (var claim in principal.FindAll("realm_access").ToArray())
        {
            using var document = ParseJsonClaim(claim);

            if (document?.RootElement.TryGetProperty("roles", out var roles) != true ||
                roles.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var role in roles.EnumerateArray())
            {
                if (role.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(role.GetString()))
                {
                    yield return role.GetString()!;
                }
            }
        }
    }

    private static IEnumerable<string> ExtractResourceRoles(ClaimsPrincipal principal)
    {
        foreach (var claim in principal.FindAll("resource_access").ToArray())
        {
            using var document = ParseJsonClaim(claim);

            if (document is null || document.RootElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var resource in document.RootElement.EnumerateObject())
            {
                if (resource.Value.TryGetProperty("roles", out var roles) &&
                    roles.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        if (role.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(role.GetString()))
                        {
                            yield return role.GetString()!;
                        }
                    }
                }
            }
        }
    }

    private static JsonDocument? ParseJsonClaim(Claim claim)
    {
        try
        {
            return JsonDocument.Parse(claim.Value);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool IsRoleClaim(Claim claim)
    {
        return claim.Type is ClaimTypes.Role or "role" or "roles";
    }
}
