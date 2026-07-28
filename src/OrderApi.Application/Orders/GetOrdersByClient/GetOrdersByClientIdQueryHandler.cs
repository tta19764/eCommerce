using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetOrdersByClient;

/// <summary>
/// Handles paginated client-order queries.
/// </summary>
public sealed class GetOrdersByClientIdQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrdersByClientIdQueryHandler> logger)
    : IQueryHandler<GetOrdersByClientIdQuery, IReadOnlyCollection<OrderResponse>>
{
    /// <summary>
    /// Reads a page of orders for a client and maps them to response models.
    /// </summary>
    /// <param name="request">The client-order query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A successful result containing the client's requested order page.</returns>
    public async Task<Result<IReadOnlyCollection<OrderResponse>>> Handle(
        GetOrdersByClientIdQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersByClientId(
            request.ClientId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = orders.Select(OrderMapper.ToResponse).ToList();

        logger.LogDebug(
            "Read order page for client {ClientId}; returned {OrderCount} orders",
            request.ClientId,
            response.Count);

        return Result.Success<IReadOnlyCollection<OrderResponse>>(response);
    }
}
