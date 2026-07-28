using MediatR;
using OrderApi.Api.Endpoints;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.CreateOrder;
using OrderApi.Application.Orders.DeleteOrder;
using OrderApi.Application.Orders.GetOrder;
using OrderApi.Application.Orders.GetOrderPage;
using OrderApi.Application.Orders.GetOrdersByClient;
using OrderApi.Application.Orders.UpdateOrder;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;

namespace OrderApi.Api.Endpoints.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("orders")
            .WithTags("Orders")
            .HasApiVersion(OrderApiApiVersions.V1);

        group.MapPost(string.Empty, CreateOrder)
            .WithName(nameof(CreateOrder))
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest);

        group.MapGet(string.Empty, GetOrders)
            .WithName(nameof(GetOrders))
            .Produces<ApiResponse<IReadOnlyCollection<OrderResponse>>>();

        group.MapGet("{orderId:guid}", GetOrder)
            .WithName(nameof(GetOrder))
            .Produces<ApiResponse<OrderDetailsResponse>>()
            .Produces<ApiResponse<OrderDetailsResponse>>(StatusCodes.Status404NotFound);

        group.MapGet("clients/{clientId:guid}", GetOrdersByClient)
            .WithName(nameof(GetOrdersByClient))
            .Produces<ApiResponse<IReadOnlyCollection<OrderResponse>>>();

        group.MapPut("{orderId:guid}", UpdateOrder)
            .WithName(nameof(UpdateOrder))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapDelete("{orderId:guid}", DeleteOrder)
            .WithName(nameof(DeleteOrder))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        return builder;
    }

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

    public static async Task<IResult> GetOrder(
        Guid orderId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderQuery(orderId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

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
}
