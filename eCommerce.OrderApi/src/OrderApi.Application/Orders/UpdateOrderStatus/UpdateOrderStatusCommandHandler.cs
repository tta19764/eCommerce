using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Defines the UpdateOrderStatusCommandHandler class used by this slice.
/// </summary>
public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<AdjustProductQuantitiesRequest> productQuantityClient,
    ILogger<UpdateOrderStatusCommandHandler> logger) : ICommandHandler<UpdateOrderStatusCommand>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found for status update", request.OrderId);
            return Result.Failure(OrderErrors.NotFound);
        }

        var previousStatus = order.Status;
        var transition = ApplyTransition(order, request.Status);

        if (transition.IsFailure)
        {
            return transition;
        }

        var adjustmentResult = await AdjustProductQuantitiesAsync(order, previousStatus, request.Status, cancellationToken);

        if (adjustmentResult.IsFailure)
        {
            return adjustmentResult;
        }

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated order {OrderId} status from {PreviousStatus} to {Status}", order.Id, previousStatus, order.Status);

        return Result.Success();
    }

    private static Result ApplyTransition(Order order, OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending when order.Status == OrderStatus.Pending => Result.Success(),
            OrderStatus.Confirmed => order.Confirm(DateTime.UtcNow),
            OrderStatus.Paid => order.MarkAsPaid(DateTime.UtcNow),
            OrderStatus.Shipped => order.MarkAsShipped(DateTime.UtcNow),
            OrderStatus.Completed => order.Complete(DateTime.UtcNow),
            OrderStatus.Cancelled => order.Cancel(DateTime.UtcNow),
            _ => Result.Failure(OrderErrors.InvalidStatusTransition)
        };
    }

    private async Task<Result> AdjustProductQuantitiesAsync(
        Order order,
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
            .Select(item => new ProductQuantityAdjustment(item.ProductId, item.Quantity.Value * quantityMultiplier))
            .ToArray();

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
