using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.Users.GetUser;
using UserApi.Domain.Users;
using Xunit;

namespace UserApi.Application.UnitTests.Users;

public class GetUserQueryHandlerTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_Should_ReturnUserResponse_WhenUserExists()
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

        var handler = new GetUserQueryHandler(
            _userRepositoryMock,
            NullLogger<GetUserQueryHandler>.Instance);

        // Act
        Result<UserApi.Application.Users.UserResponse> result =
            await handler.Handle(new GetUserQuery(user.Id), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Smith");
        result.Value.FullName.Should().Be("John Smith");
        result.Value.Email.Should().Be("john.smith@example.com");
    }
}
