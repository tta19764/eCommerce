using MassTransit;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.StartProductInquiry;

/// <summary>
/// Handles product-inquiry conversation creation.
/// </summary>
public sealed class StartProductInquiryCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetProductDetailsRequest> productClient,
    IConversationsRealtimeNotifier realtimeNotifier)
    : ICommandHandler<StartProductInquiryCommand, Guid>
{
    /// <summary>
    /// Creates a conversation for a customer and seller around one product, or returns the existing one.
    /// </summary>
    public async Task<Result<Guid>> Handle(StartProductInquiryCommand request, CancellationToken cancellationToken)
    {
        var productResponse = await productClient.GetResponse<GetProductDetailsResponse>(
            new GetProductDetailsRequest(request.ProductId),
            cancellationToken);

        if (!productResponse.Message.Found)
        {
            return Result.Failure<Guid>(ConversationErrors.ProductNotFound);
        }

        if (productResponse.Message.SellerId == Guid.Empty ||
            productResponse.Message.SellerId == request.CurrentUserId)
        {
            return Result.Failure<Guid>(ConversationErrors.InvalidSeller);
        }

        var conversation = await conversationRepository.GetProductInquiryAsync(
            request.CurrentUserId,
            productResponse.Message.SellerId,
            request.ProductId,
            cancellationToken);

        if (conversation is not null)
        {
            return Result.Success(conversation.Id);
        }

        conversation = Conversation.CreateProductInquiry(
            request.CurrentUserId,
            productResponse.Message.SellerId,
            request.ProductId,
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

