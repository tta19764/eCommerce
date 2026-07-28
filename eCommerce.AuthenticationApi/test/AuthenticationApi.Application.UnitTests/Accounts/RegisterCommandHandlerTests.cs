using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.Register;
using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace AuthenticationApi.Application.UnitTests.Accounts;

public class RegisterCommandHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IRoleRepository _roleRepositoryMock = Substitute.For<IRoleRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIdentityProvider _identityProviderMock = Substitute.For<IIdentityProvider>();

    [Fact]
    public async Task Handle_Should_RegisterIdentityAndCreateAccount()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var customerRole = Role.Customer;

        _roleRepositoryMock.GetByNameAsync("Customer", cancellationToken).Returns(customerRole);
        _identityProviderMock
            .RegisterAsync(
                Arg.Any<Guid>(),
                "john.smith@example.com",
                "password-123",
                "John",
                "Smith",
                cancellationToken)
            .Returns(callInfo => Result.Success(callInfo.Arg<Guid>().ToString()));

        var handler = new RegisterCommandHandler(
            _accountRepositoryMock,
            _roleRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock,
            NullLogger<RegisterCommandHandler>.Instance);

        var command = new RegisterCommand(
            "  john.smith@example.com  ",
            "password-123",
            "John",
            "Smith");

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _identityProviderMock.Received(1).RegisterAsync(
            result.Value,
            "john.smith@example.com",
            "password-123",
            "John",
            "Smith",
            cancellationToken);

        _accountRepositoryMock.Received(1).Add(Arg.Is<Account>(account =>
            account.Id == result.Value &&
            account.FirstName.Value == "John" &&
            account.LastName.Value == "Smith" &&
            account.Email.Value == "JOHN.SMITH@EXAMPLE.COM" &&
            account.IdentityId == result.Value.ToString() &&
            account.Roles.Any(accountRole => accountRole.RoleId == customerRole.Id)));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnDuplicateEmail_WhenAccountAlreadyExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var existingAccount = Account.Create(
            Guid.NewGuid(),
            new FirstName("John"),
            new LastName("Smith"),
            new Email("JOHN.SMITH@EXAMPLE.COM")).Value;

        _accountRepositoryMock
            .GetByEmailAsync("JOHN.SMITH@EXAMPLE.COM", cancellationToken)
            .Returns(existingAccount);

        var handler = new RegisterCommandHandler(
            _accountRepositoryMock,
            _roleRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock,
            NullLogger<RegisterCommandHandler>.Instance);

        // Act
        Result<Guid> result = await handler.Handle(
            new RegisterCommand("john.smith@example.com", "password-123", "John", "Smith"),
            cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.DuplicateEmail);

        await _identityProviderMock.DidNotReceive().RegisterAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
