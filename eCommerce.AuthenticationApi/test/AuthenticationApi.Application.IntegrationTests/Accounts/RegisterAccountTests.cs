using AuthenticationApi.Application.Accounts.Register;
using AuthenticationApi.Application.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Application.IntegrationTests.Accounts;

public sealed class RegisterAccountTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_ShouldPersistIdentityAndProfileLinks()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new RegisterCommand(
            $"user-{Guid.NewGuid():N}@test.local",
            "Password1!",
            "Test",
            "User");

        // Act
        var result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var account = await DbContext.Accounts
            .AsNoTracking()
            .SingleAsync(account => account.Id == result.Value, cancellationToken);

        account.IdentityId.Should().Be(IntegrationTestWebAppFactory.IdentitySubject);
        account.UserId.Should().NotBeNull();
    }
}
