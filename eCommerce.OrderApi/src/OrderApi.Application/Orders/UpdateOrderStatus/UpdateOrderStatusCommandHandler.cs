using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Applies administrator-controlled main-order status transitions and coordinates inventory changes.
/// </summary>
/// <remarks>
/// Confirmation removes stock and cancellation restores stock that a prior confirmation removed. The handler asks
/// ProductApi to accept the complete inventory batch before it commits the order transition. Paid status is reserved
/// for the verified payment integration path.
/// </remarks>
public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<AdjustProductQuantitiesRequest> productQuantityClient,
    ICacheService cacheService,
    ILogger<UpdateOrderStatusCommandHandler> logger) : ICommandHandler<UpdateOrderStatusCommand>
{
    /// <summary>
    /// Applies the requested lifecycle transition to an existing order.
    /// </summary>
    /// <param name="request">The command that identifies the order and requested status.</param>
    /// <param name="cancellationToken">The token that cancels repository, messaging, persistence, or cache work.</param>
    /// <returns>
    /// A success result, or a failure for a missing order, invalid transition, provider-only Paid transition,
    /// missing product, or insufficient stock.
    /// </returns>
    /// <exception cref="RequestException">The ProductApi inventory request failed or did not receive a valid response.</exception>
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.Status == OrderStatus.Paid)
        {
            return Result.Failure(OrderErrors.PaymentProviderRequired);
        }

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found for status update", request.OrderId);
            return Result.Failure(OrderErrors.NotFound);
        }

        var previousStatus = order.Status;
        var previousSellerStatuses = order.SellerOrders.ToDictionary(
            sellerOrder => sellerOrder.Id,
            sellerOrder => sellerOrder.Status);
        var transition = ApplyTransition(order, request.Status);

        if (transition.IsFailure)
        {
            return transition;
        }

        var adjustmentResult = await AdjustProductQuantitiesAsync(
            order,
            previousSellerStatuses,
            request.Status,
            cancellationToken);

        if (adjustmentResult.IsFailure)
        {
            return adjustmentResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await OrderCacheKeys.InvalidateCacheAsync(cacheService, cancellationToken);

        logger.LogInformation("Updated order {OrderId} status from {PreviousStatus} to {Status}", order.Id, previousStatus, order.Status);

        return Result.Success();
    }

    private static Result ApplyTransition(Order order, OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending when order.Status == OrderStatus.Pending => Result.Success(),
            OrderStatus.Confirmed => order.Confirm(DateTime.UtcNow),
            OrderStatus.Paid => Result.Failure(OrderErrors.PaymentProviderRequired),
            OrderStatus.Shipped => order.MarkAsShipped(DateTime.UtcNow),
            OrderStatus.Completed => order.Complete(DateTime.UtcNow),
            OrderStatus.Cancelled => order.Cancel(DateTime.UtcNow),
            _ => Result.Failure(OrderErrors.InvalidStatusTransition)
        };
    }

    private async Task<Result> AdjustProductQuantitiesAsync(
        Order order,
        IReadOnlyDictionary<Guid, OrderStatus> previousSellerStatuses,
        OrderStatus requestedStatus,
        CancellationToken cancellationToken)
    {
        var adjustments = order.Items
            .Select(item =>
            {
                var previousStatus = previousSellerStatuses.GetValueOrDefault(item.SellerOrderId, OrderStatus.Pending);
                var quantityMultiplier = GetQuantityMultiplier(previousStatus, requestedStatus);

                return quantityMultiplier == 0
                    ? null
                    : new ProductQuantityAdjustment(item.ProductId, item.Quantity.Value * quantityMultiplier);
            })
            .OfType<ProductQuantityAdjustment>()
            .ToArray();

        if (adjustments.Length == 0)
        {
            return Result.Success();
        }

        var response = await productQuantityClient.GetResponse<AdjustProductQuantitiesResponse>(
            new AdjustProductQuantitiesRequest(adjustments),
            cancellationToken);

        if (response.Message.Adjusted)
        {
            return Result.Success();
        }

        if (response.Message.MissingProductIds.Count > 0)
        {
            return Result.Failure(OrderErrors.ProductNotFound);
        }

        return Result.Failure(OrderErrors.InsufficientProductQuantity);
    }

    private static int GetQuantityMultiplier(OrderStatus previousStatus, OrderStatus requestedStatus)
    {
        if (requestedStatus == OrderStatus.Confirmed)
        {
            return -1;
        }

        if (requestedStatus == OrderStatus.Cancelled && previousStatus != OrderStatus.Pending)
        {
            return 1;
        }

        return 0;
    }
}
