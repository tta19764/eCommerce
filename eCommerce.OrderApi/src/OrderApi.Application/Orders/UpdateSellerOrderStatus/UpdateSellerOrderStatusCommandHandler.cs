using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.UpdateSellerOrderStatus;

/// <summary>
/// Handles seller-order status changes and product inventory adjustments.
/// </summary>
/// <remarks>
/// The aggregate derives the main order status from its seller-order groups. Confirmation removes stock, while
/// cancellation restores stock that a prior confirmation removed. Paid status is reserved for PaymentApi events.
/// </remarks>
public sealed class UpdateSellerOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<AdjustProductQuantitiesRequest> productQuantityClient,
    ICacheService cacheService,
    ILogger<UpdateSellerOrderStatusCommandHandler> logger) : ICommandHandler<UpdateSellerOrderStatusCommand>
{
    /// <summary>
    /// Applies a lifecycle transition to one seller-order group and coordinates its inventory changes.
    /// </summary>
    /// <param name="request">The command that identifies the seller order and requested status.</param>
    /// <param name="cancellationToken">The token that cancels repository, messaging, persistence, or cache work.</param>
    /// <returns>
    /// A success result, or a failure for a missing seller order, invalid transition, provider-only Paid transition,
    /// missing product, or insufficient stock.
    /// </returns>
    /// <exception cref="RequestException">The ProductApi inventory request failed or did not receive a valid response.</exception>
    public async Task<Result> Handle(UpdateSellerOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.Status == OrderStatus.Paid)
        {
            return Result.Failure(OrderErrors.PaymentProviderRequired);
        }

        var order = await orderRepository.GetBySellerOrderIdAsync(request.SellerOrderId, cancellationToken);
        var sellerOrder = order?.SellerOrders.FirstOrDefault(sellerOrder => sellerOrder.Id == request.SellerOrderId);

        if (order is null || sellerOrder is null)
        {
            return Result.Failure(OrderErrors.SellerOrderNotFound);
        }

        var previousStatus = sellerOrder.Status;
        var transition = order.ApplySellerOrderStatus(request.SellerOrderId, request.Status, DateTime.UtcNow);

        if (transition.IsFailure)
        {
            return transition;
        }

        var adjustmentResult = await AdjustProductQuantitiesAsync(order, request.SellerOrderId, previousStatus, request.Status, cancellationToken);

        if (adjustmentResult.IsFailure)
        {
            return adjustmentResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await OrderCacheKeys.InvalidateCacheAsync(cacheService, cancellationToken);

        logger.LogInformation(
            "Updated seller order {SellerOrderId} from {PreviousStatus} to {Status}",
            request.SellerOrderId,
            previousStatus,
            request.Status);

        return Result.Success();
    }

    private async Task<Result> AdjustProductQuantitiesAsync(
        Order order,
        Guid sellerOrderId,
        OrderStatus previousStatus,
        OrderStatus requestedStatus,
        CancellationToken cancellationToken)
    {
        var quantityMultiplier = GetQuantityMultiplier(previousStatus, requestedStatus);

        if (quantityMultiplier == 0)
        {
            return Result.Success();
        }

        var adjustments = order.Items
            .Where(item => item.SellerOrderId == sellerOrderId)
            .Select(item => new ProductQuantityAdjustment(item.ProductId, item.Quantity.Value * quantityMultiplier))
            .ToArray();

        var response = await productQuantityClient.GetResponse<AdjustProductQuantitiesResponse>(
            new AdjustProductQuantitiesRequest(adjustments),
            cancellationToken);

        if (response.Message.Adjusted)
        {
            return Result.Success();
        }

        return response.Message.MissingProductIds.Count > 0
            ? Result.Failure(OrderErrors.ProductNotFound)
            : Result.Failure(OrderErrors.InsufficientProductQuantity);
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
