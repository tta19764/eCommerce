using MessagingApi.Api.Hubs;
using MessagingApi.Api.Realtime;
using MessagingApi.Application.Conversations;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace MessagingApi.Api.UnitTests.Realtime;

public class SignalRConversationsRealtimeNotifierTests
{
    private readonly IHubClients<IConversationsHubClient> _clients;
    private readonly IConversationsHubClient _mockClient;
    private readonly SignalRConversationsRealtimeNotifier _notifier;

    public SignalRConversationsRealtimeNotifierTests()
    {
        var hubContext = Substitute.For<IHubContext<ConversationsHub, IConversationsHubClient>>();
        _clients = Substitute.For<IHubClients<IConversationsHubClient>>();
        _mockClient = Substitute.For<IConversationsHubClient>();
        
        hubContext.Clients.Returns(_clients);
        _notifier = new SignalRConversationsRealtimeNotifier(hubContext);
    }

    [Fact]
    public async Task NotifyMessageSentAsync_ShouldNotifyBothParticipants()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        
        var messageResponse = new ConversationMessageResponse(
            messageId,
            conversationId,
            customerId,
            "Hello",
            ConversationMessageType.Text,
            DateTime.UtcNow);
            
        var eventData = new MessageSentRealtimeEvent(
            conversationId,
            messageResponse,
            customerId,
            sellerId);
        
        _clients.Users(Arg.Any<IReadOnlyList<string>>()).Returns(_mockClient);

        // Act
        await _notifier.NotifyMessageSentAsync(eventData);

        // Assert
        _clients.Received(1).Users(Arg.Is<IReadOnlyList<string>>(l => 
            l.Contains(customerId.ToString()) && l.Contains(sellerId.ToString())));
        await _mockClient.Received(1).MessageSent(eventData);
    }

    [Fact]
    public async Task NotifyConversationCreatedAsync_ShouldNotifyBothParticipants()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        
        var conversationResponse = new ConversationResponse(
            conversationId,
            ConversationType.ProductInquiry,
            customerId,
            sellerId,
            Guid.NewGuid(), // ProductId
            Guid.NewGuid(), // OrderId
            Guid.NewGuid(), // SellerOrderId
            ConversationStatus.Open,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);
            
        var eventData = new ConversationCreatedRealtimeEvent(
            conversationResponse,
            customerId,
            sellerId);

        _clients.Users(Arg.Any<IReadOnlyList<string>>()).Returns(_mockClient);

        // Act
        await _notifier.NotifyConversationCreatedAsync(eventData);

        // Assert
        _clients.Received(1).Users(Arg.Is<IReadOnlyList<string>>(l => 
            l.Contains(customerId.ToString()) && l.Contains(sellerId.ToString())));
        await _mockClient.Received(1).ConversationCreated(eventData);
    }

    [Fact]
    public async Task NotifyConversationReadAsync_ShouldNotifyBothParticipants()
    {
        // Arrange
        var readerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var eventData = new ConversationReadRealtimeEvent(
            Guid.NewGuid(),
            readerId,
            otherId,
            DateTime.UtcNow);

        _clients.Users(Arg.Any<IReadOnlyList<string>>()).Returns(_mockClient);

        // Act
        await _notifier.NotifyConversationReadAsync(eventData);

        // Assert
        _clients.Received(1).Users(Arg.Is<IReadOnlyList<string>>(l => 
            l.Contains(readerId.ToString()) && l.Contains(otherId.ToString())));
        await _mockClient.Received(1).ConversationRead(eventData);
    }
}
