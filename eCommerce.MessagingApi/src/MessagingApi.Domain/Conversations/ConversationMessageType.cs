using System.Text.Json.Serialization;

namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Message type stored in a conversation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConversationMessageType
{
    Text = 1,
    System = 2
}
