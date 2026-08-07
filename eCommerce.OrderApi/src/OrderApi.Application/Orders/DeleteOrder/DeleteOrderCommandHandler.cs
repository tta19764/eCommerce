using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.DeleteOrder;

/// <summary>
/// Handles order deletion.
/// </summary>
public sealed class DeleteOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<DeleteOrderCommandHandler> logger) : ICommandHandler<DeleteOrderCommand>
{
    /// <summary>
    /// Deletes the order when it exists.
    /// </summary>
    /// <param name="request">The delete-order command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result, or a not-found failure.</returns>
    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found for deletion", request.OrderId);
            return Result.Failure(OrderErrors.NotFound);
        }

        orderRepository.Delete(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await OrderCacheKeys.InvalidateCacheAsync(cacheService, cancellationToken);

        logger.LogInformation("Deleted order {OrderId}", request.OrderId);

        return Result.Success();
    }
}
