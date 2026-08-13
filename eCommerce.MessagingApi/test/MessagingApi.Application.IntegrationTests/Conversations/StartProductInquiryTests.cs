using FluentAssertions;
using MessagingApi.Application.Conversations.StartProductInquiry;
using MessagingApi.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MessagingApi.Application.IntegrationTests.Conversations;

public sealed class StartProductInquiryTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task StartProductInquiry_ShouldPersistAndReuseConversation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new StartProductInquiryCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var firstResult = await Sender.Send(command, cancellationToken);
        var secondResult = await Sender.Send(command, cancellationToken);

        // Assert
        firstResult.Value.Should().Be(secondResult.Value);

        var conversationCount = await DbContext.Conversations.CountAsync(cancellationToken);
        conversationCount.Should().Be(1);
    }
}
