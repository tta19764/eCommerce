using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Domain errors produced by conversation operations.
/// </summary>
public static class ConversationErrors
{
    public static readonly Error NotFound = new("Conversation.NotFound", "Conversation was not found");
    public static readonly Error Forbidden = new("Conversation.Forbidden", "Current user cannot access this conversation");
    public static readonly Error EmptyMessage = new("Conversation.EmptyMessage", "Message body cannot be empty");
    public static readonly Error ProductNotFound = new("Conversation.ProductNotFound", "Product was not found");
    public static readonly Error SellerOrderNotFound = new("Conversation.SellerOrderNotFound", "Seller order was not found");
    public static readonly Error InvalidSeller = new("Conversation.InvalidSeller", "Conversation requires a different seller participant");
}
