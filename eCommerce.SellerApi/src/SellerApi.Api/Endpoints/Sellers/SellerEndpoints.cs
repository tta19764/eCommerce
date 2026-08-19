using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using SellerApi.Application.Sellers;
using SellerApi.Application.Sellers.ApproveSeller;
using SellerApi.Application.Sellers.GetOwnSeller;
using SellerApi.Application.Sellers.GetPendingSellers;
using SellerApi.Application.Sellers.RejectSeller;
using SellerApi.Application.Sellers.SubmitSellerApplication;
using SellerApi.Domain.Sellers;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Application.Authorization;
using SharedLibrary.Application.Pagination;
using System.Security.Claims;

namespace SellerApi.Api.Endpoints.Sellers;

/// <summary>Defines seller application endpoints.</summary>
public static class SellerEndpoints
{
    /// <summary>Maps authenticated owner endpoints and permission-protected review endpoints.</summary>
    /// <param name="builder">The endpoint route builder to update.</param>
    /// <returns>The same endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapSellerEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("sellers").WithTags("Sellers").HasApiVersion(SellerApiVersions.V1);
        group.MapPost("own/application", SubmitApplication)
            .WithName(nameof(SubmitApplication))
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        group.MapGet("own", GetOwn)
            .WithName(nameof(GetOwn))
            .Produces<ApiResponse<SellerResponse>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<SellerResponse>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("pending", GetPending)
            .WithName(nameof(GetPending))
            .Produces<ApiResponse<PagedListResponse<PendingSellerApplicationResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(ApplicationPermissions.SellerReview);

        group.MapPost("{sellerId:guid}/approve", Approve)
            .WithName(nameof(Approve))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(ApplicationPermissions.SellerReview);

        group.MapPost("{sellerId:guid}/reject", Reject)
            .WithName(nameof(Reject))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(ApplicationPermissions.SellerReview);

        return builder;
    }

    /// <summary>Submits a pending seller and store application for the authenticated user.</summary>
    /// <param name="request">The proposed public store data.</param>
    /// <param name="sender">The sender that dispatches the application command.</param>
    /// <param name="principal">The authenticated principal.</param>
    /// <param name="accounts">The AuthenticationApi client that resolves the principal to a UserApi identifier.</param>
    /// <param name="cancellationToken">The token that cancels identity resolution and command processing.</param>
    /// <returns>A created result, a validation failure, or a forbidden result when identity resolution fails.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">AuthenticationApi does not return an identity response.</exception>
    private static async Task<IResult> SubmitApplication(StoreApplicationRequest request, ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var userId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (userId is null) return Results.Forbid();

        var result = await sender.Send(new SubmitSellerApplicationCommand(userId.Value, request.Slug, request.Name, request.Description, request.CountryCode, request.DefaultCurrency), cancellationToken);
        return result.IsSuccess
            ? Results.Created("/api/v1/sellers/own", result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>Gets the current user's seller or the shared marketplace seller for an administrator.</summary>
    /// <param name="sender">The sender that dispatches the query.</param>
    /// <param name="principal">The authenticated principal used for identity and role resolution.</param>
    /// <param name="accounts">The AuthenticationApi client that resolves the principal to a UserApi identifier.</param>
    /// <param name="cancellationToken">The token that cancels identity resolution and query processing.</param>
    /// <returns>An OK result, a not-found result, or a forbidden result when identity resolution fails.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">AuthenticationApi does not return an identity response.</exception>
    private static async Task<IResult> GetOwn(ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var userId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (userId is null) return Results.Forbid();
        var isAdmin = principal.IsInRole(ApplicationRoles.Admin.Name);
        var result = await sender.Send(
            new GetOwnSellerQuery(userId.Value, isAdmin),
            cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>Gets one normalized page of pending seller applications for administrative review.</summary>
    /// <param name="page">The requested one-based page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="sender">The sender that dispatches the query.</param>
    /// <param name="cancellationToken">The token that cancels query processing.</param>
    /// <returns>An OK result that contains the enriched pending-application page.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    private static async Task<IResult> GetPending(int page, int pageSize, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPendingSellersQuery(page, pageSize), cancellationToken);
        return Results.Ok(result.MapToApiResponse());
    }

    /// <summary>Approves one pending seller application as the authenticated administrator.</summary>
    /// <param name="sellerId">The seller application identifier.</param>
    /// <param name="sender">The sender that dispatches the command.</param>
    /// <param name="principal">The authenticated administrator principal.</param>
    /// <param name="accounts">The AuthenticationApi client that resolves the administrator's UserApi identifier.</param>
    /// <param name="cancellationToken">The token that cancels identity resolution and command processing.</param>
    /// <returns>No content on success, not found for an unknown seller, bad request for an invalid state, or forbidden when identity resolution fails.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">AuthenticationApi does not return an identity response.</exception>
    private static async Task<IResult> Approve(Guid sellerId, ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var adminId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (adminId is null) return Results.Forbid();
        var result = await sender.Send(new ApproveSellerCommand(sellerId, adminId.Value), cancellationToken);
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error == SellerErrors.NotFound
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>Rejects one pending seller application as the authenticated administrator.</summary>
    /// <param name="sellerId">The seller application identifier.</param>
    /// <param name="request">The administrator's rejection reason.</param>
    /// <param name="sender">The sender that dispatches the command.</param>
    /// <param name="principal">The authenticated administrator principal.</param>
    /// <param name="accounts">The AuthenticationApi client that resolves the administrator's UserApi identifier.</param>
    /// <param name="cancellationToken">The token that cancels identity resolution and command processing.</param>
    /// <returns>No content on success, not found for an unknown seller, bad request for invalid input or state, or forbidden when identity resolution fails.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">AuthenticationApi does not return an identity response.</exception>
    private static async Task<IResult> Reject(Guid sellerId, RejectSellerRequest request, ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var adminId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (adminId is null) return Results.Forbid();
        var result = await sender.Send(new RejectSellerCommand(sellerId, adminId.Value, request.Reason), cancellationToken);
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error == SellerErrors.NotFound
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }
}
