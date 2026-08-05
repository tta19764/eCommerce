using MassTransit;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using OrderApi.Messages.Orders;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.StartSellerOrderConversation;

/// <summary>
/// Handles seller-order conversation creation.
/// </summary>
public sealed class StartSellerOrderConversationCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetSellerOrderConversationDetailsRequest> sellerOrderClient,
    IConversationsRealtimeNotifier realtimeNotifier)
    : ICommandHandler<StartSellerOrderConversationCommand, Guid>
{
    /// <summary>
    /// Creates a conversation for the customer and seller who own a seller-order group.
    /// </summary>
    public async Task<Result<Guid>> Handle(StartSellerOrderConversationCommand request, CancellationToken cancellationToken)
    {
        var details = await sellerOrderClient.GetResponse<GetSellerOrderConversationDetailsResponse>(
            new GetSellerOrderConversationDetailsRequest(request.SellerOrderId),
            cancellationToken);

        if (!details.Message.Found)
        {
            return Result.Failure<Guid>(ConversationErrors.SellerOrderNotFound);
        }

        if (request.CurrentUserId != details.Message.CustomerUserId &&
            request.CurrentUserId != details.Message.SellerUserId)
        {
            return Result.Failure<Guid>(ConversationErrors.Forbidden);
        }

        var conversation = await conversationRepository.GetSellerOrderConversationAsync(
            request.SellerOrderId,
            cancellationToken);

        if (conversation is not null)
        {
            return Result.Success(conversation.Id);
        }

        conversation = Conversation.CreateSellerOrderConversation(
            details.Message.CustomerUserId,
            details.Message.SellerUserId,
            details.Message.OrderId,
            request.SellerOrderId,
            DateTime.UtcNow);

        conversationRepository.Add(conversation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.NotifyConversationCreatedAsync(
            new ConversationCreatedRealtimeEvent(
                new ConversationResponse(
                    conversation.Id,
                    conversation.Type,
                    conversation.CustomerUserId,
                    conversation.SellerUserId,
                    conversation.ProductId,
                    conversation.OrderId,
                    conversation.SellerOrderId,
                    conversation.Status,
                    conversation.CreatedAtUtc,
                    conversation.LastMessageAtUtc,
                    conversation.CustomerReadAtUtc,
                    conversation.SellerReadAtUtc),
                conversation.CustomerUserId,
                conversation.SellerUserId),
            cancellationToken);

        return Result.Success(conversation.Id);
    }
}

