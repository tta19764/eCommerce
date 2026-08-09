using FluentAssertions;
using MessagingApi.Domain.Conversations;
using Xunit;

namespace MessagingApi.Domain.UnitTests.Conversations;

public sealed class ConversationTests
{
    [Fact]
    public void CreateProductInquiry_ShouldInitializeOpenConversation()
    {
        var customerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var conversation = Conversation.CreateProductInquiry(customerId, sellerId, productId, createdAtUtc);

        conversation.Type.Should().Be(ConversationType.ProductInquiry);
        conversation.CustomerUserId.Should().Be(customerId);
        conversation.SellerUserId.Should().Be(sellerId);
        conversation.ProductId.Should().Be(productId);
        conversation.Status.Should().Be(ConversationStatus.Open);
        conversation.LastMessageAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void AddMessage_ShouldTrimBodyAndAdvanceLastMessageTime()
    {
        var conversation = CreateConversation();
        var sentAtUtc = DateTime.UtcNow.AddMinutes(1);

        var result = conversation.AddMessage(
            conversation.CustomerUserId,
            "  Hello seller  ",
            ConversationMessageType.Text,
            sentAtUtc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Body.Should().Be("Hello seller");
        result.Value.SenderUserId.Should().Be(conversation.CustomerUserId);
        conversation.LastMessageAtUtc.Should().Be(sentAtUtc);
        conversation.Messages.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddMessage_ShouldRejectEmptyBody(string body)
    {
        var conversation = CreateConversation();

        var result = conversation.AddMessage(null, body, ConversationMessageType.System, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ConversationErrors.EmptyMessage);
        conversation.Messages.Should().BeEmpty();
    }

    [Fact]
    public void AddSystemMessage_ShouldCreateSenderlessSystemMessage()
    {
        var conversation = CreateConversation();

        var result = conversation.AddSystemMessage("Order confirmed", DateTime.UtcNow);

        result.Value.SenderUserId.Should().BeNull();
        result.Value.Type.Should().Be(ConversationMessageType.System);
        conversation.HasSystemMessage("Order confirmed").Should().BeTrue();
    }

    [Fact]
    public void MarkRead_ShouldOnlyUpdateMatchingParticipant()
    {
        var conversation = CreateConversation();
        var readAtUtc = DateTime.UtcNow;

        conversation.MarkRead(conversation.CustomerUserId, readAtUtc);

        conversation.CustomerReadAtUtc.Should().Be(readAtUtc);
        conversation.SellerReadAtUtc.Should().BeNull();
    }

    [Fact]
    public void HasParticipant_ShouldRejectUnrelatedUser()
    {
        CreateConversation().HasParticipant(Guid.NewGuid()).Should().BeFalse();
    }

    private static Conversation CreateConversation()
    {
        return Conversation.CreateSellerOrderConversation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
    }
}
