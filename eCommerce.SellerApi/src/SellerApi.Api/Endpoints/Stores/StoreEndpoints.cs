using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using SellerApi.Application.Stores.CreateStoreReview;
using SellerApi.Application.Stores.GetStore;
using SellerApi.Application.Stores.GetStoreReviews;
using SellerApi.Application.Sellers;
using SellerApi.Application.Stores;
using SellerApi.Domain.Stores;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Api.Contracts;
using System.Security.Claims;

namespace SellerApi.Api.Endpoints.Stores;

/// <summary>Defines public store and store-review endpoints.</summary>
public static class StoreEndpoints
{
    /// <summary>Maps public store reads and authenticated store-review creation.</summary>
    /// <param name="builder">The endpoint route builder to update.</param>
    /// <returns>The same endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("stores").WithTags("Stores").HasApiVersion(SellerApiVersions.V1);
        group.MapGet("{slug}", GetStore)
            .WithName(nameof(GetStore))
            .Produces<ApiResponse<StoreResponse>>()
            .Produces<ApiResponse<StoreResponse>>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapGet("{storeId:guid}/reviews", GetReviews)
            .WithName(nameof(GetReviews))
            .Produces<ApiResponse<IReadOnlyList<StoreReviewResponse>>>()
            .AllowAnonymous();

        group.MapPost("{storeId:guid}/reviews", CreateReview)
            .WithName(nameof(CreateReview))
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return builder;
    }

    /// <summary>Gets one active public store by slug.</summary>
    /// <param name="slug">The public store slug.</param>
    /// <param name="sender">The sender that dispatches the query.</param>
    /// <param name="cancellationToken">The token that cancels query processing.</param>
    /// <returns>An OK result for an active store, or a not-found result.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    private static async Task<IResult> GetStore(string slug, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreQuery(slug), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>Gets one newest-first page of reviews without checking store visibility.</summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="page">The requested one-based page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="sender">The sender that dispatches the query.</param>
    /// <param name="cancellationToken">The token that cancels query processing.</param>
    /// <returns>An OK result. An unknown store produces an empty list.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    private static async Task<IResult> GetReviews(Guid storeId, int page, int pageSize, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreReviewsQuery(storeId, page, pageSize), cancellationToken);
        return Results.Ok(result.MapToApiResponse());
    }

    /// <summary>Creates a review after OrderApi verifies the completed purchase.</summary>
    /// <param name="storeId">The reviewed store identifier.</param>
    /// <param name="request">The seller-order identifier, rating, and review text.</param>
    /// <param name="sender">The sender that dispatches the command.</param>
    /// <param name="principal">The authenticated customer principal.</param>
    /// <param name="accounts">The AuthenticationApi client that resolves the customer's UserApi identifier.</param>
    /// <param name="cancellationToken">The token that cancels identity resolution, verification, and persistence.</param>
    /// <returns>A created result, a domain failure, or a forbidden result when identity resolution fails.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">AuthenticationApi does not return an identity response.</exception>
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

        return result.Error == StoreErrors.NotFound
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }
}
