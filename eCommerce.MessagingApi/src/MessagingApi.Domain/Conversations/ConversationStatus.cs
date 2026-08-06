using System.Text.Json.Serialization;

namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Conversation lifecycle status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConversationStatus
{
    Open = 1,
    Closed = 2
}
