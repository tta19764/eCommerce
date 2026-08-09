using FluentAssertions;
using MassTransit;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations.Messaging;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Messages.Orders;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace MessagingApi.Api.UnitTests.Conversations;

public sealed class SellerOrderStatusChangedConsumerTests
{
    private readonly IConversationRepository _conversationRepository = Substitute.For<IConversationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IConversationsRealtimeNotifier _realtimeNotifier = Substitute.For<IConversationsRealtimeNotifier>();

    [Fact]
    public async Task Consume_ShouldAddSystemMessageAndNotifyParticipants_WhenConversationExists()
    {
        var sellerOrderId = Guid.Parse("9451ca76-1111-2222-3333-444444444444");
        var changedAtUtc = DateTime.UtcNow;
        var conversation = Conversation.CreateSellerOrderConversation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            sellerOrderId,
            changedAtUtc.AddMinutes(-1));
        var context = CreateContext(new SellerOrderStatusChangedIntegrationEvent(
            conversation.OrderId!.Value,
            sellerOrderId,
            conversation.CustomerUserId,
            conversation.SellerUserId,
            "Confirmed",
            changedAtUtc));

        _conversationRepository
            .GetSellerOrderConversationAsync(sellerOrderId, context.CancellationToken)
            .Returns(conversation);

        var consumer = CreateConsumer();

        await consumer.Consume(context);

        var message = conversation.Messages.Should().ContainSingle().Which;
        message.SenderUserId.Should().BeNull();
        message.Type.Should().Be(ConversationMessageType.System);
        message.Body.Should().Be("The state of the order #**9451ca76 has been changed to: Confirmed**");
        message.CreatedAtUtc.Should().Be(changedAtUtc);
        await _unitOfWork.Received(1).SaveChangesAsync(context.CancellationToken);
        await _realtimeNotifier.Received(1).NotifyMessageSentAsync(
            Arg.Is<MessageSentRealtimeEvent>(notification =>
                notification.ConversationId == conversation.Id &&
                notification.Message.Id == message.Id &&
                notification.Message.Type == ConversationMessageType.System),
            context.CancellationToken);
    }

    [Fact]
    public async Task Consume_ShouldNotAddDuplicate_WhenStatusMessageAlreadyExists()
    {
        var sellerOrderId = Guid.Parse("9451ca76-1111-2222-3333-444444444444");
        var conversation = Conversation.CreateSellerOrderConversation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), sellerOrderId, DateTime.UtcNow);
        conversation.AddSystemMessage(
            "The state of the order #**9451ca76 has been changed to: Confirmed**",
            DateTime.UtcNow);
        var context = CreateContext(new SellerOrderStatusChangedIntegrationEvent(
            conversation.OrderId!.Value,
            sellerOrderId,
            conversation.CustomerUserId,
            conversation.SellerUserId,
            "Confirmed",
            DateTime.UtcNow));

        _conversationRepository
            .GetSellerOrderConversationAsync(sellerOrderId, context.CancellationToken)
            .Returns(conversation);

        await CreateConsumer().Consume(context);

        conversation.Messages.Should().ContainSingle();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _realtimeNotifier.DidNotReceive().NotifyMessageSentAsync(
            Arg.Any<MessageSentRealtimeEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldCreateConversation_WhenStatusChangesBeforeChatIsOpened()
    {
        var sellerOrderId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var sellerUserId = Guid.NewGuid();
        var context = CreateContext(new SellerOrderStatusChangedIntegrationEvent(
            Guid.NewGuid(),
            sellerOrderId,
            customerUserId,
            sellerUserId,
            "Paid",
            DateTime.UtcNow));

        await CreateConsumer().Consume(context);

        _conversationRepository.Received(1).Add(Arg.Is<Conversation>(conversation =>
            conversation.SellerOrderId == sellerOrderId &&
            conversation.CustomerUserId == customerUserId &&
            conversation.SellerUserId == sellerUserId &&
            conversation.Messages.Single().Type == ConversationMessageType.System));
        await _unitOfWork.Received(1).SaveChangesAsync(context.CancellationToken);
        await _realtimeNotifier.Received(1).NotifyConversationCreatedAsync(
            Arg.Is<ConversationCreatedRealtimeEvent>(notification =>
                notification.Conversation.SellerOrderId == sellerOrderId),
            context.CancellationToken);
        await _realtimeNotifier.Received(1).NotifyMessageSentAsync(
            Arg.Is<MessageSentRealtimeEvent>(notification =>
                notification.Message.Body.Contains("Paid")),
            context.CancellationToken);
    }

    private SellerOrderStatusChangedConsumer CreateConsumer()
    {
        return new SellerOrderStatusChangedConsumer(
            _conversationRepository,
            _unitOfWork,
            _realtimeNotifier,
            NullLogger<SellerOrderStatusChangedConsumer>.Instance);
    }

    private static ConsumeContext<SellerOrderStatusChangedIntegrationEvent> CreateContext(
        SellerOrderStatusChangedIntegrationEvent message)
    {
        var context = Substitute.For<ConsumeContext<SellerOrderStatusChangedIntegrationEvent>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }
}
