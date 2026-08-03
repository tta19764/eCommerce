using System.Security.Claims;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using OrderApi.Api.Endpoints;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.CreateOrder;
using OrderApi.Application.Orders.DeleteOrder;
using OrderApi.Application.Orders.GetOrder;
using OrderApi.Application.Orders.GetOrderPage;
using OrderApi.Application.Orders.GetOrdersByClient;
using OrderApi.Application.Orders.UpdateOrder;
using OrderApi.Application.Orders.UpdateOrderStatus;
using OrderApi.Domain.Orders;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Application.Authorization;
using SharedLibrary.Application.Pagination;

namespace OrderApi.Api.Endpoints.Orders;

/// <summary>
/// Defines the OrderEndpoints class used by this slice.
/// </summary>
public static class OrderEndpoints
{
    /// <summary>
    /// Executes the MapOrderEndpoints operation.
    /// </summary>
    /// <param name="builder">The builder value.</param>
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("orders")
            .WithTags("Orders")
            .HasApiVersion(OrderApiApiVersions.V1);

        group.MapPost(string.Empty, CreateOrder)
            .WithName(nameof(CreateOrder))
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(ApplicationPermissions.OrderCreate);

        group.MapPost("own", CreateOwnOrder)
            .WithName(nameof(CreateOwnOrder))
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(ApplicationPermissions.OrderCreate);

        group.MapGet(string.Empty, GetOrders)
            .WithName(nameof(GetOrders))
            .Produces<ApiResponse<PagedListResponse<OrderResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(ApplicationPermissions.OrderRead);

        group.MapGet("{orderId:guid}", GetOrder)
            .WithName(nameof(GetOrder))
            .Produces<ApiResponse<OrderDetailsResponse>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<OrderDetailsResponse>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("clients/{clientId:guid}", GetOrdersByClient)
            .WithName(nameof(GetOrdersByClient))
            .Produces<ApiResponse<PagedListResponse<OrderResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(ApplicationPermissions.OrderRead);

        group.MapGet("own", GetOwnOrders)
            .WithName(nameof(GetOwnOrders))
            .Produces<ApiResponse<PagedListResponse<OrderResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization();

        group.MapPut("{orderId:guid}", UpdateOrder)
            .WithName(nameof(UpdateOrder))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(ApplicationPermissions.OrderUpdateStatus);

        group.MapPatch("{orderId:guid}/status", UpdateOrderStatus)
            .WithName(nameof(UpdateOrderStatus))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(ApplicationPermissions.OrderUpdateStatus);

        group.MapPost("{orderId:guid}/cancel", CancelOwnOrder)
            .WithName(nameof(CancelOwnOrder))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapDelete("{orderId:guid}", DeleteOrder)
            .WithName(nameof(DeleteOrder))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(ApplicationPermissions.OrderUpdateStatus);

        return builder;
    }

    /// <summary>
    /// Executes the CreateOrder operation.
    /// </summary>
    /// <param name="command">The command value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> CreateOrder(
        CreateOrderCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(
                nameof(GetOrder),
                new { orderId = result.Value, version = OrderApiApiVersions.V1RouteValue },
                result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the CreateOwnOrder operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="accountClient">The accountClient value.</param>
    /// <param name="user">The user value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> CreateOwnOrder(
        CreateOwnOrderRequest request,
        ISender sender,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(new CreateOrderCommand(currentUserId.Value, request.Items), cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(
                nameof(GetOrder),
                new { orderId = result.Value, version = OrderApiApiVersions.V1RouteValue },
                result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the GetOrders operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> GetOrders(
        [AsParameters] GetOrdersRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrderPageQuery(
                request.Page,
                request.PageSize,
                request.MinOrderPrice,
                request.MaxOrderPrice,
                request.SortByOrderPrice,
                request.SortDescending),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the GetOrder operation.
    /// </summary>
    /// <param name="orderId">The orderId value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="user">The user value.</param>
    /// <param name="authorizationService">The authorizationService value.</param>
    /// <param name="accountClient">The accountClient value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> GetOrder(
        Guid orderId,
        ISender sender,
        ClaimsPrincipal user,
        IAuthorizationService authorizationService,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderQuery(orderId), cancellationToken);

        if (result.IsFailure)
        {
            return Results.NotFound(result.MapToApiResponse());
        }

        if (await HasPermissionAsync(user, authorizationService, ApplicationPermissions.OrderRead) ||
            await IsCurrentUserAsync(user, result.Value.ClientId, accountClient, cancellationToken))
        {
            return Results.Ok(result.MapToApiResponse());
        }

        return Results.Forbid();
    }

    /// <summary>
    /// Executes the GetOrdersByClient operation.
    /// </summary>
    /// <param name="clientId">The clientId value.</param>
    /// <param name="request">The request value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> GetOrdersByClient(
        Guid clientId,
        [AsParameters] GetClientOrdersRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOrdersByClientIdQuery(clientId, request.Page, request.PageSize),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the GetOwnOrders operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="accountClient">The accountClient value.</param>
    /// <param name="user">The user value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> GetOwnOrders(
        [AsParameters] GetClientOrdersRequest request,
        ISender sender,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new GetOrdersByClientIdQuery(currentUserId.Value, request.Page, request.PageSize),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the UpdateOrder operation.
    /// </summary>
    /// <param name="orderId">The orderId value.</param>
    /// <param name="request">The request value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> UpdateOrder(
        Guid orderId,
        UpdateOrderRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateOrderCommand(orderId, request.Items), cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the UpdateOrderStatus operation.
    /// </summary>
    /// <param name="orderId">The orderId value.</param>
    /// <param name="request">The request value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> UpdateOrderStatus(
        Guid orderId,
        UpdateOrderStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateOrderStatusCommand(orderId, request.Status), cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the CancelOwnOrder operation.
    /// </summary>
    /// <param name="orderId">The orderId value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="user">The user value.</param>
    /// <param name="accountClient">The accountClient value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> CancelOwnOrder(
        Guid orderId,
        ISender sender,
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var order = await sender.Send(new GetOrderQuery(orderId), cancellationToken);

        if (order.IsFailure)
        {
            return Results.NotFound(order.MapToApiResponse());
        }

        if (order.Value.ClientId != currentUserId.Value)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(new UpdateOrderStatusCommand(orderId, OrderStatus.Cancelled), cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the DeleteOrder operation.
    /// </summary>
    /// <param name="orderId">The orderId value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> DeleteOrder(
        Guid orderId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOrderCommand(orderId), cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.MapToApiResponse());
    }

    private static async Task<bool> HasPermissionAsync(
        ClaimsPrincipal user,
        IAuthorizationService authorizationService,
        string permission)
    {
        var result = await authorizationService.AuthorizeAsync(user, permission);
        return result.Succeeded;
    }

    private static async Task<bool> IsCurrentUserAsync(
        ClaimsPrincipal user,
        Guid userId,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        return await GetCurrentUserIdAsync(user, accountClient, cancellationToken) == userId;
    }

    private static async Task<Guid?> GetCurrentUserIdAsync(
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
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
