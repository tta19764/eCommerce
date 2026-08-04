using SharedLibrary.Application.Abstractions.Messaging;

namespace MessagingApi.Application.Conversations.StartProductInquiry;

/// <summary>
/// Starts or reuses a product inquiry conversation between the current customer and the product seller.
/// </summary>
public sealed record StartProductInquiryCommand(Guid CurrentUserId, Guid ProductId) : ICommand<Guid>;

