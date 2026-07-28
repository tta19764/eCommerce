using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.Orders.UpdateOrder;

/// <summary>
/// Handles replacing items on an existing pending order.
/// </summary>
public sealed class UpdateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetProductDetailsRequest> productClient,
    ILogger<UpdateOrderCommandHandler> logger) : ICommandHandler<UpdateOrderCommand>
{
    /// <summary>
    /// Loads the order, fetches fresh product snapshots, replaces pending items, and persists the changes.
    /// </summary>
    /// <param name="request">The update-order command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result, or a failure result when the order/product is missing or the transition is invalid.</returns>
    public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found for update", request.OrderId);
            return Result.Failure(OrderErrors.NotFound);
        }

        var replacementItems = new List<OrderItem>();

        foreach (var item in request.Items.GroupBy(item => item.ProductId).Select(group => new OrderItemRequest(group.Key, group.Sum(item => item.Quantity))))
        {
            // Replacing items intentionally refreshes product snapshots while the order is still pending.
            var product = await productClient.GetResponse<GetProductDetailsResponse>(
                new GetProductDetailsRequest(item.ProductId),
                cancellationToken);

            if (!product.Message.Found)
            {
                logger.LogWarning("Product {ProductId} was not found while updating order {OrderId}", item.ProductId, request.OrderId);
                return Result.Failure(OrderErrors.ProductNotFound);
            }

            replacementItems.Add(OrderItem.Create(
                order.Id,
                product.Message.ProductId,
                new ProductName(product.Message.Name),
                new Money(product.Message.Price, Currency.FromCode(product.Message.Currency)),
                new OrderItemQuantity(item.Quantity)));
        }

        var updateResult = order.ReplaceItems(replacementItems);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated order {OrderId}", request.OrderId);

        return Result.Success();
    }
}
