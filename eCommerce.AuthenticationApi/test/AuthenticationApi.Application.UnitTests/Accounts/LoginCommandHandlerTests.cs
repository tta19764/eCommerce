using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.Login;
using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace AuthenticationApi.Application.UnitTests.Accounts;

public class LoginCommandHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IIdentityProvider _identityProviderMock = Substitute.For<IIdentityProvider>();

    [Fact]
    public async Task Handle_Should_ReturnAccessToken_WhenAccountExistsAndIdentityProviderAuthenticates()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var account = Account.Create(
            Guid.NewGuid(),
            new Email("JOHN.SMITH@EXAMPLE.COM"),
            new PasswordHash("EXTERNAL_IDENTITY_PROVIDER")).Value;

        var tokenResponse = new TokenResponse("access-token", DateTime.UtcNow.AddMinutes(5));

        _accountRepositoryMock
            .GetByEmailAsync("JOHN.SMITH@EXAMPLE.COM", cancellationToken)
            .Returns(account);

        _identityProviderMock
            .LoginAsync("john.smith@example.com", "password-123", cancellationToken)
            .Returns(Result.Success(tokenResponse));

        var handler = new LoginCommandHandler(_accountRepositoryMock, _identityProviderMock);
        var command = new LoginCommand("  john.smith@example.com  ", "password-123");

        // Act
        Result<TokenResponse> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tokenResponse);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidCredentials_WhenAccountDoesNotExist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var handler = new LoginCommandHandler(_accountRepositoryMock, _identityProviderMock);

        // Act
        Result<TokenResponse> result = await handler.Handle(
            new LoginCommand("missing@example.com", "password-123"),
            cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InvalidCredentials);

        await _identityProviderMock
            .DidNotReceive()
            .LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
