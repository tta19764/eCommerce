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
/// <param name="conversationRepository">The repository that finds or tracks conversations.</param>
/// <param name="unitOfWork">The unit of work that persists a new conversation.</param>
/// <param name="productClient">The ProductApi client that resolves the product seller.</param>
/// <param name="realtimeNotifier">The notifier that broadcasts newly created conversations.</param>
/// <remarks>
/// The current user becomes the customer participant. A user cannot start an inquiry for a missing product, a
/// product without a seller, or their own product. The database unique index is the final concurrency guard for
/// the customer, seller, and product tuple.
/// </remarks>
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
    /// <param name="request">The product and authenticated current-user identifiers.</param>
    /// <param name="cancellationToken">The token that cancels service lookup, persistence, and notification.</param>
    /// <returns>
    /// The existing or created conversation identifier on success. A failure result indicates a missing product
    /// or an invalid seller relationship.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// The database commit occurs before real-time notification. A notification failure can therefore propagate
    /// after the conversation has been created. Repeating the command then returns the existing identifier without
    /// repeating the creation notification.
    /// </remarks>
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

