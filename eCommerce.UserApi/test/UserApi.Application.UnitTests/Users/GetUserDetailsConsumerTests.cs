using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using UserApi.Application.Users.Messaging;
using UserApi.Domain.Users;
using UserApi.Messages.Users;
using Xunit;

namespace UserApi.Application.UnitTests.Users;

public class GetUserDetailsConsumerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Consume_Should_ReturnUserDetails_WhenUserExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var user = User.Create(
            new FirstName("John"),
            new LastName("Smith"),
            new Email("john.smith@example.com")).Value;

        _userRepositoryMock
            .GetByIdAsync(user.Id, cancellationToken)
            .Returns(user);

        GetUserDetailsResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetUserDetailsRequest>>();
        context.Message.Returns(new GetUserDetailsRequest(user.Id));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<GetUserDetailsResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new GetUserDetailsConsumer(
            _userRepositoryMock,
            NullLogger<GetUserDetailsConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.Found.Should().BeTrue();
        response.UserId.Should().Be(user.Id);
        response.FullName.Should().Be("John Smith");
        response.Email.Should().Be("john.smith@example.com");
    }

    [Fact]
    public async Task Consume_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .GetByIdAsync(userId, cancellationToken)
            .Returns((User?)null);

        GetUserDetailsResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetUserDetailsRequest>>();
        context.Message.Returns(new GetUserDetailsRequest(userId));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<GetUserDetailsResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new GetUserDetailsConsumer(
            _userRepositoryMock,
            NullLogger<GetUserDetailsConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.Found.Should().BeFalse();
        response.UserId.Should().Be(userId);
        response.FullName.Should().BeEmpty();
        response.Email.Should().BeEmpty();
    }
}
