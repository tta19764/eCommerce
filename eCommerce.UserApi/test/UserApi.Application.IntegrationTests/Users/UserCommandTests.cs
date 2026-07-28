using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.IntegrationTests.Infrastructure;
using UserApi.Application.Users.CreateUser;
using UserApi.Application.Users.UpdateUser;
using Xunit;

namespace UserApi.Application.IntegrationTests.Users;

public class UserCommandTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateUser_Should_PersistUser()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateUserCommand("John", "Smith", $"john.smith.{Guid.NewGuid():N}@example.com");

        // Act
        Result<Guid> result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var user = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == result.Value, cancellationToken);

        user.Should().NotBeNull();
        user.FirstName.Value.Should().Be(command.FirstName);
        user.LastName.Value.Should().Be(command.LastName);
        user.Email.Value.Should().Be(command.Email);
        user.FullName.Should().Be("John Smith");
    }

    [Fact]
    public async Task UpdateUser_Should_UpdatePersistedUser()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var email = $"john.smith.{Guid.NewGuid():N}@example.com";
        var createResult = await Sender.Send(
            new CreateUserCommand("John", "Smith", email),
            cancellationToken);

        createResult.IsSuccess.Should().BeTrue();
        DbContext.ChangeTracker.Clear();

        var command = new UpdateUserCommand(
            createResult.Value,
            "Jane",
            "Doe",
            "image-123");

        // Act
        Result result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var user = await DbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == createResult.Value, cancellationToken);

        user.Should().NotBeNull();
        user.FirstName.Value.Should().Be(command.FirstName);
        user.LastName.Value.Should().Be(command.LastName);
        user.Email.Value.Should().Be(email);
        user.FullName.Should().Be("Jane Doe");
        user.ImageId.Should().Be("image-123");
    }
}
