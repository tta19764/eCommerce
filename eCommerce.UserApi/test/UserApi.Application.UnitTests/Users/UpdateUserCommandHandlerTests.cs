using FluentAssertions;
using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.Users.UpdateUser;
using UserApi.Domain.Users;
using Xunit;

namespace UserApi.Application.UnitTests.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IRequestClient<AddUserImageRequest> _imageClientMock =
        Substitute.For<IRequestClient<AddUserImageRequest>>();

    [Fact]
    public async Task Handle_Should_UpdateUserAndSaveChanges_WhenUserExists()
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

        var imageId = Guid.NewGuid();
        SetupValidImagesResponse(imageId);

        var handler = new UpdateUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            _imageClientMock,
            NullLogger<UpdateUserCommandHandler>.Instance);

        var command = new UpdateUserCommand(user.Id, "  Jane  ", "  Doe  ", imageId);

        // Act
        Result result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FirstName.Value.Should().Be("Jane");
        user.LastName.Value.Should().Be("Doe");
        user.Email.Value.Should().Be("john.smith@example.com");
        user.ImageId.Should().Be(imageId);

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .GetByIdAsync(userId, cancellationToken)
            .Returns((User?)null);

        var handler = new UpdateUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            _imageClientMock,
            NullLogger<UpdateUserCommandHandler>.Instance);

        // Act
        Result result = await handler.Handle(
            new UpdateUserCommand(userId, "Jane", "Doe", Guid.NewGuid()),
            cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);

        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void SetupValidImagesResponse(Guid imageId)
    {
        var response = Substitute.For<Response<AddUserImageResponse>>();
        response.Message.Returns(new AddUserImageResponse(true, imageId, []));

        _imageClientMock
            .GetResponse<AddUserImageResponse>(
                Arg.Is<AddUserImageRequest>(request => request.TemporaryImageId == imageId),
                Arg.Any<CancellationToken>())
            .Returns(response);
    }
}
