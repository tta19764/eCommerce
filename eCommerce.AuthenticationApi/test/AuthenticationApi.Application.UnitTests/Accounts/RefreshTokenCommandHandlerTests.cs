using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.RefreshToken;
using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace AuthenticationApi.Application.UnitTests.Accounts;

public class RefreshTokenCommandHandlerTests
{
    private readonly IIdentityProvider _identityProviderMock = Substitute.For<IIdentityProvider>();

    [Fact]
    public async Task Handle_Should_ReturnTokens_WhenIdentityProviderRefreshesToken()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var tokenResponse = new TokenResponse(
            "new-access-token",
            DateTime.UtcNow.AddMinutes(5),
            "new-refresh-token",
            DateTime.UtcNow.AddDays(7));

        _identityProviderMock
            .RefreshTokenAsync("refresh-token", cancellationToken)
            .Returns(Result.Success(tokenResponse));

        var handler = new RefreshTokenCommandHandler(_identityProviderMock);
        var command = new RefreshTokenCommand("  refresh-token  ");

        // Act
        Result<TokenResponse> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tokenResponse);
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidCredentials_WhenIdentityProviderRejectsRefreshToken()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        _identityProviderMock
            .RefreshTokenAsync("expired-refresh-token", cancellationToken)
            .Returns(Result.Failure<TokenResponse>(AccountErrors.InvalidCredentials));

        var handler = new RefreshTokenCommandHandler(_identityProviderMock);

        // Act
        Result<TokenResponse> result = await handler.Handle(
            new RefreshTokenCommand("expired-refresh-token"),
            cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InvalidCredentials);
    }
}
