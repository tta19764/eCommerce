using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using SellerApi.Application.Sellers.ApproveSeller;
using SellerApi.Application.Sellers.GetOwnSeller;
using SellerApi.Application.Sellers.GetPendingSellers;
using SellerApi.Application.Sellers.RejectSeller;
using SellerApi.Application.Sellers.SubmitSellerApplication;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Application.Authorization;
using System.Security.Claims;

namespace SellerApi.Api.Endpoints.Sellers;

/// <summary>Defines seller application endpoints.</summary>
public static class SellerEndpoints
{
    /// <summary>Maps seller application endpoints.</summary>
    public static IEndpointRouteBuilder MapSellerEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("sellers").WithTags("Sellers").HasApiVersion(SellerApiVersions.V1);
        group.MapPost("own/application", SubmitApplication).RequireAuthorization();
        group.MapGet("own", GetOwn).RequireAuthorization();
        group.MapGet("pending", GetPending).RequireAuthorization(ApplicationPermissions.SellerReview);
        group.MapPost("{sellerId:guid}/approve", Approve).RequireAuthorization(ApplicationPermissions.SellerReview);
        group.MapPost("{sellerId:guid}/reject", Reject).RequireAuthorization(ApplicationPermissions.SellerReview);
        return builder;
    }

    /// <summary>Submits a store application for the current user.</summary>
    private static async Task<IResult> SubmitApplication(StoreApplicationRequest request, ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var userId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (userId is null) return Results.Forbid();

        var result = await sender.Send(new SubmitSellerApplicationCommand(userId.Value, request.Slug, request.Name, request.Description, request.CountryCode, request.DefaultCurrency), cancellationToken);
        return result.IsSuccess
            ? Results.Created("/api/v1/sellers/own", result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>Gets the current user's seller application.</summary>
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

    /// <summary>Gets one page of pending seller applications.</summary>
    private static async Task<IResult> GetPending(int page, int pageSize, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPendingSellersQuery(page, pageSize), cancellationToken);
        return Results.Ok(result.MapToApiResponse());
    }

    /// <summary>Approves one pending seller application.</summary>
    private static async Task<IResult> Approve(Guid sellerId, ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var adminId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (adminId is null) return Results.Forbid();
        var result = await sender.Send(new ApproveSellerCommand(sellerId, adminId.Value), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>Rejects one pending seller application.</summary>
    private static async Task<IResult> Reject(Guid sellerId, RejectSellerRequest request, ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var adminId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (adminId is null) return Results.Forbid();
        var result = await sender.Send(new RejectSellerCommand(sellerId, adminId.Value, request.Reason), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.MapToApiResponse());
    }
}
