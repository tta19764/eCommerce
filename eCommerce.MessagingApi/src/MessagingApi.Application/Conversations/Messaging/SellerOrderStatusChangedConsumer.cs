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
public sealed class SellerOrderStatusChangedConsumer(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IConversationsRealtimeNotifier realtimeNotifier,
    ILogger<SellerOrderStatusChangedConsumer> logger)
    : IConsumer<SellerOrderStatusChangedIntegrationEvent>
{
    /// <inheritdoc />
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
