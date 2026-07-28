using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using UserApi.Application.IntegrationTests.Infrastructure;
using UserApi.Application.Users.CreateUser;
using UserApi.Application.Users.Messaging;
using UserApi.Domain.Users;
using UserApi.Infrastructure.Repositories;
using UserApi.Messages.Users;
using Xunit;

namespace UserApi.Application.IntegrationTests.Users;

public class GetUserDetailsConsumerTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Consume_Should_ReturnPersistedUserDetails()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var email = $"john.smith.{Guid.NewGuid():N}@example.com";
        Guid userId = (await Sender.Send(
            new CreateUserCommand("John", "Smith", email),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        GetUserDetailsResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetUserDetailsRequest>>();
        context.Message.Returns(new GetUserDetailsRequest(userId));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<GetUserDetailsResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new GetUserDetailsConsumer(
            new UserRepository(DbContext),
            NullLogger<GetUserDetailsConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.Found.Should().BeTrue();
        response.UserId.Should().Be(userId);
        response.FullName.Should().Be("John Smith");
        response.Email.Should().Be(email);
    }
}
