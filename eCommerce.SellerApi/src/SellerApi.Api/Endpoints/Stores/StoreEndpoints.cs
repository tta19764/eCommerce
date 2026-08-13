using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using SellerApi.Application.Stores.CreateStoreReview;
using SellerApi.Application.Stores.GetStore;
using SellerApi.Application.Stores.GetStoreReviews;
using SellerApi.Application.Sellers;
using SharedLibrary.Api.Extensions;
using System.Security.Claims;

namespace SellerApi.Api.Endpoints.Stores;

/// <summary>Defines public store and store-review endpoints.</summary>
public static class StoreEndpoints
{
    /// <summary>Maps public store and store-review endpoints.</summary>
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("stores").WithTags("Stores").HasApiVersion(SellerApiVersions.V1);
        group.MapGet("{slug}", GetStore);
        group.MapGet("{storeId:guid}/reviews", GetReviews);
        group.MapPost("{storeId:guid}/reviews", CreateReview).RequireAuthorization();
        return builder;
    }

    /// <summary>Gets one active public store by slug.</summary>
    private static async Task<IResult> GetStore(string slug, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreQuery(slug), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>Gets one page of reviews for a store.</summary>
    private static async Task<IResult> GetReviews(Guid storeId, int page, int pageSize, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreReviewsQuery(storeId, page, pageSize), cancellationToken);
        return Results.Ok(result.MapToApiResponse());
    }

    /// <summary>Creates a review after OrderApi verifies the completed purchase.</summary>
    private static async Task<IResult> CreateReview(Guid storeId, CreateStoreReviewRequest request, ISender sender, ClaimsPrincipal principal, IRequestClient<GetAccountUserIdByIdentityIdRequest> accounts, CancellationToken cancellationToken)
    {
        var userId = await principal.GetCurrentUserIdAsync(accounts, cancellationToken);
        if (userId is null) return Results.Forbid();

        var result = await sender.Send(new CreateStoreReviewCommand(storeId, userId.Value, request.SellerOrderId, request.Rating, request.Comment), cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Created(
                $"/api/v1/stores/{storeId}/reviews/{result.Value}",
                result.MapToApiResponse());
        }

        return result.Error == SellerApplicationErrors.StoreNotFound
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }
}
