using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Messages.Orders;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.Users.DeleteUser;
using UserApi.Domain.Users;
using Xunit;

namespace UserApi.Application.UnitTests.Users;

public class DeleteUserCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IRequestClient<HasOrdersForClientRequest> _ordersClientMock =
        Substitute.For<IRequestClient<HasOrdersForClientRequest>>();

    [Fact]
    public async Task Handle_Should_DeleteUser_WhenUserHasNoOrders()
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

        _ordersClientMock
            .GetResponse<HasOrdersForClientResponse>(
                Arg.Is<HasOrdersForClientRequest>(request => request.ClientId == user.Id),
                cancellationToken)
            .Returns(Task.FromResult<Response<HasOrdersForClientResponse>>(
                new TestResponse<HasOrdersForClientResponse>(
                    new HasOrdersForClientResponse(user.Id, false))));

        var handler = new DeleteUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            _ordersClientMock,
            NullLogger<DeleteUserCommandHandler>.Instance);

        // Act
        Result result = await handler.Handle(new DeleteUserCommand(user.Id), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Received(1).Delete(user);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnHasOrdersAndSkipDelete_WhenOrdersExist()
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

        _ordersClientMock
            .GetResponse<HasOrdersForClientResponse>(
                Arg.Is<HasOrdersForClientRequest>(request => request.ClientId == user.Id),
                cancellationToken)
            .Returns(Task.FromResult<Response<HasOrdersForClientResponse>>(
                new TestResponse<HasOrdersForClientResponse>(
                    new HasOrdersForClientResponse(user.Id, true))));

        var handler = new DeleteUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            _ordersClientMock,
            NullLogger<DeleteUserCommandHandler>.Instance);

        // Act
        Result result = await handler.Handle(new DeleteUserCommand(user.Id), cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.HasOrders);

        _userRepositoryMock.DidNotReceive().Delete(Arg.Any<User>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundAndSkipOrdersRequest_WhenUserDoesNotExist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .GetByIdAsync(userId, cancellationToken)
            .Returns((User?)null);

        var handler = new DeleteUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            _ordersClientMock,
            NullLogger<DeleteUserCommandHandler>.Instance);

        // Act
        Result result = await handler.Handle(new DeleteUserCommand(userId), cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);

        await _ordersClientMock.DidNotReceive()
            .GetResponse<HasOrdersForClientResponse>(
                Arg.Any<HasOrdersForClientRequest>(),
                Arg.Any<CancellationToken>());
        _userRepositoryMock.DidNotReceive().Delete(Arg.Any<User>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
