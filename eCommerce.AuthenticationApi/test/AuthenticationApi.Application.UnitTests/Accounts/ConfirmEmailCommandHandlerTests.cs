using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.ConfirmEmail;
using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace AuthenticationApi.Application.UnitTests.Accounts;

public class ConfirmEmailCommandHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIdentityProvider _identityProviderMock = Substitute.For<IIdentityProvider>();

    [Fact]
    public async Task Handle_Should_ConfirmEmail_WhenAccountAndEmailMatch()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var account = Account.Create(
            Guid.NewGuid(),
            new Email("JOHN.SMITH@EXAMPLE.COM")).Value;
        account.SetIdentityId("keycloak-user-id");

        _accountRepositoryMock
            .GetByIdAsync(account.Id, cancellationToken)
            .Returns(account);

        _identityProviderMock
            .ConfirmEmailAsync("keycloak-user-id", cancellationToken)
            .Returns(Result.Success());

        var handler = new ConfirmEmailCommandHandler(
            _accountRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock);

        // Act
        Result result = await handler.Handle(
            new ConfirmEmailCommand(account.Id, "john.smith@example.com"),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.IsEmailConfirmed.Should().BeTrue();

        await _identityProviderMock.Received(1).ConfirmEmailAsync("keycloak-user-id", cancellationToken);
        _accountRepositoryMock.Received(1).Update(account);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmailMismatch_WhenEmailDoesNotMatchAccount()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var account = Account.Create(
            Guid.NewGuid(),
            new Email("JOHN.SMITH@EXAMPLE.COM")).Value;

        _accountRepositoryMock
            .GetByIdAsync(account.Id, cancellationToken)
            .Returns(account);

        var handler = new ConfirmEmailCommandHandler(
            _accountRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock);

        // Act
        Result result = await handler.Handle(
            new ConfirmEmailCommand(account.Id, "other@example.com"),
            cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.EmailMismatch);

        await _identityProviderMock
            .DidNotReceive()
            .ConfirmEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
