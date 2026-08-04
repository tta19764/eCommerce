using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.UpdateSellerOrderStatus;

/// <summary>
/// Handles seller-order status changes and product inventory adjustments.
/// </summary>
public sealed class UpdateSellerOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<AdjustProductQuantitiesRequest> productQuantityClient,
    ILogger<UpdateSellerOrderStatusCommandHandler> logger) : ICommandHandler<UpdateSellerOrderStatusCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateSellerOrderStatusCommand request, CancellationToken cancellationToken)
    {
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

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
