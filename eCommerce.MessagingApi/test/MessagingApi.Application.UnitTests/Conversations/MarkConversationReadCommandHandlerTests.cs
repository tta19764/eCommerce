using FluentAssertions;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations.MarkConversationRead;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace MessagingApi.Application.UnitTests.Conversations;

public sealed class MarkConversationReadCommandHandlerTests
{
    private readonly IConversationRepository _repository = Substitute.For<IConversationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IConversationsRealtimeNotifier _notifier = Substitute.For<IConversationsRealtimeNotifier>();

    [Fact]
    public async Task Handle_ShouldMarkParticipantReadAndNotifyBothUsers()
    {
        var conversation = CreateConversation();
        var cancellationToken = CancellationToken.None;
        _repository.GetByIdAsync(conversation.Id, cancellationToken).Returns(conversation);
        var handler = new MarkConversationReadCommandHandler(_repository, _unitOfWork, _notifier);

        var result = await handler.Handle(
            new MarkConversationReadCommand(conversation.CustomerUserId, conversation.Id),
            cancellationToken);

        result.IsSuccess.Should().BeTrue();
        conversation.CustomerReadAtUtc.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(cancellationToken);
        await _notifier.Received(1).NotifyConversationReadAsync(
            Arg.Is<ConversationReadRealtimeEvent>(message =>
                message.ConversationId == conversation.Id &&
                message.ReaderUserId == conversation.CustomerUserId &&
                message.OtherParticipantUserId == conversation.SellerUserId),
            cancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbiddenForNonParticipantWithoutSaving()
    {
        var conversation = CreateConversation();
        var cancellationToken = CancellationToken.None;
        _repository.GetByIdAsync(conversation.Id, cancellationToken).Returns(conversation);
        var handler = new MarkConversationReadCommandHandler(_repository, _unitOfWork, _notifier);

        var result = await handler.Handle(
            new MarkConversationReadCommand(Guid.NewGuid(), conversation.Id),
            cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ConversationErrors.Forbidden);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Conversation CreateConversation() => Conversation.CreateProductInquiry(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
}
