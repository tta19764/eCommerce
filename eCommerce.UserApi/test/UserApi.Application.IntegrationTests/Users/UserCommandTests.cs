using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Domain.Abstractions;
using UserApi.Application.IntegrationTests.Infrastructure;
using UserApi.Application.Users.CreateUser;
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
}
