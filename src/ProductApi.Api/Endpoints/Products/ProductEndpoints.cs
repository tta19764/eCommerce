using MediatR;
using ProductApi.Application.Products;
using ProductApi.Application.Products.CreateProduct;
using ProductApi.Application.Products.DeleteProduct;
using ProductApi.Application.Products.GetProduct;
using ProductApi.Application.Products.GetProductPage;
using ProductApi.Application.Products.UpdateProduct;
using ProductApi.Application.Reviews;
using ProductApi.Application.Reviews.CreateProductReview;
using ProductApi.Application.Reviews.GetProductReviewsPage;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Application.Pagination;

namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Minimal API endpoints for product catalog management.
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Maps product catalog endpoints.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder with product endpoints registered.</returns>
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("products")
            .WithTags("Products")
            .HasApiVersion(ProductApiApiVersions.V1);

        group.MapPost(string.Empty, CreateProduct)
            .WithName(nameof(CreateProduct))
            .WithSummary("Create a product")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet(string.Empty, GetProducts)
            .WithName(nameof(GetProducts))
            .WithSummary("Get products by page")
            .Produces<ApiResponse<PagedListResponse<ProductResponse>>>()
            .Produces<ApiResponse<PagedListResponse<ProductResponse>>>(StatusCodes.Status400BadRequest);

        group.MapGet("{productId:guid}", GetProduct)
            .WithName(nameof(GetProduct))
            .WithSummary("Get product details")
            .Produces<ApiResponse<ProductResponse>>()
            .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status404NotFound);

        group.MapPut("{productId:guid}", UpdateProduct)
            .WithName(nameof(UpdateProduct))
            .WithSummary("Update product details")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapDelete("{productId:guid}", DeleteProduct)
            .WithName(nameof(DeleteProduct))
            .WithSummary("Delete a product")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPost("{productId:guid}/reviews", CreateProductReview)
            .WithName(nameof(CreateProductReview))
            .WithSummary("Create a product review")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("{productId:guid}/reviews", GetProductReviewsPage)
            .WithName(nameof(GetProductReviewsPage))
            .WithSummary("Get product reviews by page")
            .Produces<ApiResponse<PagedListResponse<ProductReviewResponse>>>()
            .Produces<ApiResponse<PagedListResponse<ProductReviewResponse>>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<PagedListResponse<ProductReviewResponse>>>(StatusCodes.Status404NotFound);

        return builder;
    }

    /// <summary>
    /// Gets a page of products.
    /// </summary>
    /// <param name="request">The pagination request values.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result containing a page of products or validation errors.</returns>
    public static async Task<IResult> GetProducts(
        [AsParameters] GetProductsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductPageQuery(request.Page, request.PageSize),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Creates a product.
    /// </summary>
    /// <param name="command">The product creation command.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result containing the created product identifier or validation errors.</returns>
    public static async Task<IResult> CreateProduct(
        CreateProductCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(
                nameof(GetProduct),
                new { productId = result.Value, version = ProductApiApiVersions.V1RouteValue },
                result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Gets a single product by identifier.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result containing product details or a not-found error.</returns>
    public static async Task<IResult> GetProduct(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductQuery(productId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The product update request body.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result indicating success, validation failure, or not-found failure.</returns>
    public static async Task<IResult> UpdateProduct(
        Guid productId,
        UpdateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            productId,
            request.Name,
            request.Description,
            request.Price,
            request.CurrencyCode,
            request.Quantity,
            request.ImageIds);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        // Not-found is a business failure, while all other product errors are client validation failures.
        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Deletes an existing product.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result indicating success or not-found failure.</returns>
    public static async Task<IResult> DeleteProduct(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCommand(productId), cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>
    /// Creates a product review and updates the product rating summary.
    /// </summary>
    /// <param name="productId">The reviewed product identifier.</param>
    /// <param name="request">The review request body.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result containing the created review identifier or validation errors.</returns>
    public static async Task<IResult> CreateProductReview(
        Guid productId,
        CreateProductReviewRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateProductReviewCommand(
                productId,
                request.UserId,
                request.Rating,
                request.Comment),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.CreatedAtRoute(
                nameof(GetProductReviewsPage),
                new { productId, version = ProductApiApiVersions.V1RouteValue },
                result.MapToApiResponse());
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Gets a page of reviews for a product.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The pagination request values.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result containing a page of product reviews or validation errors.</returns>
    public static async Task<IResult> GetProductReviewsPage(
        Guid productId,
        [AsParameters] GetProductReviewsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetProductReviewsPageQuery(productId, request.Page, request.PageSize),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.MapToApiResponse());
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }
}
