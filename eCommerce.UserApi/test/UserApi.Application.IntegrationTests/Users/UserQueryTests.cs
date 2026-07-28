using FluentAssertions;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.IntegrationTests.Infrastructure;
using UserApi.Application.Users;
using UserApi.Application.Users.CreateUser;
using UserApi.Application.Users.GetUser;
using Xunit;

namespace UserApi.Application.IntegrationTests.Users;

public class UserQueryTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetUser_Should_ReturnPersistedUser()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var email = $"john.smith.{Guid.NewGuid():N}@example.com";
        Guid userId = (await Sender.Send(
            new CreateUserCommand("John", "Smith", email),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        // Act
        Result<UserResponse> result = await Sender.Send(new GetUserQuery(userId), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userId);
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Smith");
        result.Value.FullName.Should().Be("John Smith");
        result.Value.Email.Should().Be(email);
    }
}
