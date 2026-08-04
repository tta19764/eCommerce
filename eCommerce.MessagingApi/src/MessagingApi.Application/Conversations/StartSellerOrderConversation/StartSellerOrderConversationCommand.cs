using SharedLibrary.Application.Abstractions.Messaging;

namespace MessagingApi.Application.Conversations.StartSellerOrderConversation;

/// <summary>
/// Starts or reuses a conversation attached to one seller-order group.
/// </summary>
public sealed record StartSellerOrderConversationCommand(Guid CurrentUserId, Guid SellerOrderId) : ICommand<Guid>;

