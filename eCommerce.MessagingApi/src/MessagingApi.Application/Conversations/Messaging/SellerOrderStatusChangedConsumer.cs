using MassTransit;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using Microsoft.Extensions.Logging;
using OrderApi.Messages.Orders;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.Messaging;

/// <summary>
/// Adds seller-order status changes to an existing customer-seller conversation.
/// </summary>
/// <param name="conversationRepository">The repository that finds or tracks the seller-order conversation.</param>
/// <param name="unitOfWork">The unit of work that persists new conversations and system messages.</param>
/// <param name="realtimeNotifier">The notifier that broadcasts created conversations and system messages.</param>
/// <param name="logger">The logger that records duplicates and domain failures.</param>
/// <remarks>
/// A status event creates the seller-order conversation when necessary. Duplicate suppression compares the exact
/// generated message body, which contains the first eight hexadecimal characters of the seller-order identifier
/// and the status text. Repeated transitions to the same status therefore create only one system message.
/// </remarks>
public sealed class SellerOrderStatusChangedConsumer(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IConversationsRealtimeNotifier realtimeNotifier,
    ILogger<SellerOrderStatusChangedConsumer> logger)
    : IConsumer<SellerOrderStatusChangedIntegrationEvent>
{
    /// <summary>Adds an idempotent system message for a seller-order status event.</summary>
    /// <param name="context">The consume context that contains participants, order identifiers, status, and time.</param>
    /// <returns>A task that completes after persistence and real-time notifications, or after a handled no-op.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    /// <remarks>
    /// Database persistence occurs before SignalR notification. A notification failure can cause message
    /// redelivery, but the exact-body check prevents another stored system message.
    /// </remarks>
    public async Task Consume(ConsumeContext<SellerOrderStatusChangedIntegrationEvent> context)
    {
        var statusChanged = context.Message;
        var conversation = await conversationRepository.GetSellerOrderConversationAsync(
            statusChanged.SellerOrderId,
            context.CancellationToken);
        var conversationCreated = conversation is null;
        conversation ??= Conversation.CreateSellerOrderConversation(
            statusChanged.CustomerUserId,
            statusChanged.SellerUserId,
            statusChanged.OrderId,
            statusChanged.SellerOrderId,
            statusChanged.ChangedAtUtc);

        if (conversationCreated)
        {
            conversationRepository.Add(conversation);
        }

        var body = CreateMessageBody(statusChanged.SellerOrderId, statusChanged.Status);

        if (conversation.HasSystemMessage(body))
        {
            logger.LogDebug(
                "Conversation {ConversationId} already contains seller order status {Status}",
                conversation.Id,
                statusChanged.Status);
            return;
        }

        var addResult = conversation.AddSystemMessage(body, statusChanged.ChangedAtUtc);

        if (addResult.IsFailure)
        {
            logger.LogWarning(
                "Could not add seller order status message to conversation {ConversationId}: {Error}",
                conversation.Id,
                addResult.Error);
            return;
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        if (conversationCreated)
        {
            await realtimeNotifier.NotifyConversationCreatedAsync(
                new ConversationCreatedRealtimeEvent(
                    ConversationMapper.ToResponse(conversation),
                    conversation.CustomerUserId,
                    conversation.SellerUserId),
                context.CancellationToken);
        }

        await realtimeNotifier.NotifyMessageSentAsync(
            new MessageSentRealtimeEvent(
                conversation.Id,
                ConversationMapper.ToResponse(addResult.Value),
                conversation.CustomerUserId,
                conversation.SellerUserId),
            context.CancellationToken);
    }

    private static string CreateMessageBody(Guid sellerOrderId, string status)
    {
        var shortOrderId = sellerOrderId.ToString("N")[..8];
        return $"The state of the order #{shortOrderId} has been changed to: {status}";
    }
}
