using System.Text.Json.Serialization;

namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Type of marketplace conversation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConversationType
{
    ProductInquiry = 1,
    SellerOrder = 2
}
