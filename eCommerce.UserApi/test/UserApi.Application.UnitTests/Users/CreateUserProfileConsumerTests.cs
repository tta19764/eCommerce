using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.Users.Messaging;
using UserApi.Domain.Users;
using UserApi.Messages.Users;
using Xunit;

namespace UserApi.Application.UnitTests.Users;

public class CreateUserProfileConsumerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Consume_Should_CreateProfileLinkedToIdentity()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var identityId = Guid.NewGuid();
        CreateUserProfileResponse? response = null;

        var context = Substitute.For<ConsumeContext<CreateUserProfileRequest>>();
        context.Message.Returns(new CreateUserProfileRequest(identityId, "John", "Smith", "john.smith@example.com"));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<CreateUserProfileResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new CreateUserProfileConsumer(
            _userRepositoryMock,
            _unitOfWorkMock,
            NullLogger<CreateUserProfileConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.Created.Should().BeTrue();
        response.UserId.Should().NotBeEmpty();

        _userRepositoryMock.Received(1).Add(Arg.Is<User>(user =>
            user.Id == response.UserId &&
            user.IdentityId == identityId.ToString() &&
            user.FirstName.Value == "John" &&
            user.LastName.Value == "Smith" &&
            user.Email.Value == "john.smith@example.com"));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }
}
