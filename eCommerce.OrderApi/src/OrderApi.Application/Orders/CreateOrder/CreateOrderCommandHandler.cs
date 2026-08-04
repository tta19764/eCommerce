using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.Orders.CreateOrder;

/// <summary>
/// Handles order creation and captures product details through ProductApi message requests.
/// </summary>
public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetProductDetailsRequest> productClient,
    ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, Guid>
{
    /// <summary>
    /// Creates a pending order, merges duplicate product lines, stores product snapshots, and persists the aggregate.
    /// </summary>
    /// <param name="request">The create-order command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The created order identifier, or a failure result.</returns>
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order for client {ClientId}", request.ClientId);

        var order = Order.Create(request.ClientId, new OrderDate(DateTime.UtcNow));

        foreach (var item in request.Items.GroupBy(item => item.ProductId).Select(group => new OrderItemRequest(group.Key, group.Sum(item => item.Quantity))))
        {
            // Product data is copied into the order so later product changes do not rewrite order history.
            var product = await productClient.GetResponse<GetProductDetailsResponse>(
                new GetProductDetailsRequest(item.ProductId),
                cancellationToken);

            if (!product.Message.Found)
            {
                logger.LogWarning("Product {ProductId} was not found while creating an order", item.ProductId);
                return Result.Failure<Guid>(OrderErrors.ProductNotFound);
            }

            var addItemResult = order.AddItem(
                product.Message.SellerId,
                product.Message.ProductId,
                new ProductName(product.Message.Name),
                new Money(product.Message.Price, Currency.FromCode(product.Message.Currency)),
                new OrderItemQuantity(item.Quantity));

            if (addItemResult.IsFailure)
            {
                return Result.Failure<Guid>(addItemResult.Error);
            }
        }

        if (order.Items.Count == 0)
        {
            return Result.Failure<Guid>(OrderErrors.EmptyOrder);
        }

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created order {OrderId}", order.Id);

        return Result.Success(order.Id);
    }
}
