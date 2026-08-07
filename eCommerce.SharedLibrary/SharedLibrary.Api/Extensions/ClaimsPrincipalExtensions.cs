using System.Security.Claims;
using AuthenticationApi.Messages.Accounts;
using MassTransit;

namespace SharedLibrary.Api.Extensions;

/// <summary>
/// Extension methods for retrieving user identification from HTTP ClaimsPrincipal and MassTransit Account service.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the application User API Profile identifier associated with the ClaimsPrincipal identity claims.
    /// </summary>
    /// <param name="user">The HTTP ClaimsPrincipal from the request context.</param>
    /// <param name="accountClient">The MassTransit request client for GetAccountUserIdByIdentityIdRequest.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resolved User API profile Guid, or null if unauthenticated or not found.</returns>
    public static async Task<Guid?> GetCurrentUserIdAsync(
        this ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken = default)
    {
        var identityId = user.FindFirstValue("identity_id") ??
                         user.FindFirstValue("IdentityId") ??
                         user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                         user.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(identityId))
        {
            return null;
        }

        var response = await accountClient.GetResponse<GetAccountUserIdByIdentityIdResponse>(
            new GetAccountUserIdByIdentityIdRequest(identityId),
            cancellationToken);

        return response.Message.Found
            ? response.Message.UserId
            : null;
    }
}
