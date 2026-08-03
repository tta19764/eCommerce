using AuthenticationApi.Messages.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApi.Messages.Emails;
using OrderApi.Domain.Orders;
using UserApi.Messages.Users;

namespace OrderApi.Application.Orders.Notifications;

/// <summary>
/// Sends order status change notifications for confirmed customer email addresses.
/// </summary>
public sealed class OrderStatusChangedNotificationDispatcher(
    IOrderRepository orderRepository,
    IRequestClient<GetAccountContactByUserIdRequest> accountClient,
    IRequestClient<GetUserDetailsRequest> userClient,
    IPublishEndpoint publishEndpoint,
    ILogger<OrderStatusChangedNotificationDispatcher> logger)
{
    /// <summary>
    /// Publishes an order status notification request when the order owner has confirmed email.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="status">The new order status.</param>
    /// <param name="changedAtUtc">The UTC status change time.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    public async Task DispatchAsync(
        Guid orderId,
        OrderStatus status,
        DateTime? changedAtUtc,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found for status notification", orderId);
            return;
        }

        var account = await GetAccountAsync(order.ClientId, cancellationToken);

        if (account is null)
        {
            return;
        }

        if (!account.IsEmailConfirmed)
        {
            logger.LogInformation(
                "Skipping order {OrderId} status notification because account {AccountId} email is not confirmed",
                orderId,
                account.AccountId);

            return;
        }

        var fullName = await GetFullNameAsync(order.ClientId, cancellationToken);
        var effectiveChangedAtUtc = changedAtUtc ?? GetChangedAtUtc(order, status) ?? DateTime.UtcNow;

        await publishEndpoint.Publish(
            new SendOrderStatusChangedRequest(
                order.Id,
                account.Email,
                fullName,
                status.ToString(),
                effectiveChangedAtUtc),
            cancellationToken);

        logger.LogInformation(
            "Queued order {OrderId} status notification for {Email}",
            order.Id,
            account.Email);
    }

    private async Task<GetAccountContactByUserIdResponse?> GetAccountAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await accountClient.GetResponse<GetAccountContactByUserIdResponse>(
                new GetAccountContactByUserIdRequest(userId),
                cancellationToken);

            if (!response.Message.Found)
            {
                logger.LogWarning("Account linked to order user {UserId} was not found", userId);
                return null;
            }

            return response.Message;
        }
        catch (Exception exception) when (exception is RequestException or RequestTimeoutException)
        {
            logger.LogWarning(
                exception,
                "Could not read account contact information for order user {UserId}",
                userId);

            return null;
        }
    }

    private async Task<string> GetFullNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await userClient.GetResponse<GetUserDetailsResponse>(
                new GetUserDetailsRequest(userId),
                cancellationToken);

            return response.Message.Found
                ? response.Message.FullName
                : string.Empty;
        }
        catch (Exception exception) when (exception is RequestException or RequestTimeoutException)
        {
            logger.LogWarning(
                exception,
                "Could not read user profile details for order user {UserId}",
                userId);

            return string.Empty;
        }
    }

    private static DateTime? GetChangedAtUtc(Order order, OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Confirmed => order.ConfirmedOnUtc,
            OrderStatus.Paid => order.PaidOnUtc,
            OrderStatus.Shipped => order.ShippedOnUtc,
            OrderStatus.Completed => order.CompletedOnUtc,
            OrderStatus.Cancelled => order.CancelledOnUtc,
            _ => null
        };
    }
}
