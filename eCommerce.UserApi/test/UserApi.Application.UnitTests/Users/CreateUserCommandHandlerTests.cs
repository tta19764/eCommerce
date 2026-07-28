using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.Users.CreateUser;
using UserApi.Domain.Users;
using Xunit;

namespace UserApi.Application.UnitTests.Users;

public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_Should_AddUserAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var handler = new CreateUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            NullLogger<CreateUserCommandHandler>.Instance);

        var command = new CreateUserCommand("  John  ", "  Smith  ", "  john.smith@example.com  ");

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _userRepositoryMock.Received(1).Add(Arg.Is<User>(user =>
            user.Id == result.Value &&
            user.FirstName.Value == "John" &&
            user.LastName.Value == "Smith" &&
            user.Email.Value == "john.smith@example.com"));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }
}
