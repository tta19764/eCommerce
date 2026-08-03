using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.RegisterAdmin;
using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationApi.Messages.Emails;
using NSubstitute;
using SharedLibrary.Application.Authorization;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;
using Xunit;

namespace AuthenticationApi.Application.UnitTests.Accounts;

public class RegisterAdminCommandHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IRoleRepository _roleRepositoryMock = Substitute.For<IRoleRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIdentityProvider _identityProviderMock = Substitute.For<IIdentityProvider>();
    private readonly IRequestClient<CreateUserProfileRequest> _userProfileClientMock =
        Substitute.For<IRequestClient<CreateUserProfileRequest>>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();

    [Fact]
    public async Task Handle_Should_RegisterIdentityAndCreateAdminAccount()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var adminRole = Role.Admin;
        const string identityId = "keycloak-admin-user-id";

        _roleRepositoryMock.GetByNameAsync(ApplicationRoles.Admin, cancellationToken).Returns(adminRole);
        _identityProviderMock
            .RegisterAsync(
                Arg.Any<Guid>(),
                "admin@example.com",
                "password-123",
                "Admin",
                "User",
                ApplicationRoles.Admin,
                cancellationToken)
            .Returns(Result.Success(identityId));

        _userProfileClientMock
            .GetResponse<CreateUserProfileResponse>(
                Arg.Any<CreateUserProfileRequest>(),
                cancellationToken)
            .Returns(new TestResponse<CreateUserProfileResponse>(
                new CreateUserProfileResponse(Guid.Parse("22222222-2222-2222-2222-222222222222"), true, null, null)));

        var handler = new RegisterAdminCommandHandler(
            _accountRepositoryMock,
            _roleRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock,
            _userProfileClientMock,
            _publishEndpointMock,
            NullLogger<RegisterAdminCommandHandler>.Instance);

        var command = new RegisterAdminCommand(
            "  admin@example.com  ",
            "password-123",
            "Admin",
            "User");

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _identityProviderMock.Received(1).RegisterAsync(
            result.Value,
            "admin@example.com",
            "password-123",
            "Admin",
            "User",
            ApplicationRoles.Admin,
            cancellationToken);

        _accountRepositoryMock.Received(1).Add(Arg.Is<Account>(account =>
            account.Id == result.Value &&
            account.Email.Value == "ADMIN@EXAMPLE.COM" &&
            account.IdentityId == identityId &&
            account.UserId == Guid.Parse("22222222-2222-2222-2222-222222222222") &&
            account.Roles.Any(accountRole => accountRole.RoleId == adminRole.Id)));

        await _unitOfWorkMock.Received(2).SaveChangesAsync(cancellationToken);

        await _userProfileClientMock.Received(1).GetResponse<CreateUserProfileResponse>(
            Arg.Is<CreateUserProfileRequest>(request =>
                request.FirstName == "Admin" &&
                request.LastName == "User" &&
                request.Email == "admin@example.com"),
            cancellationToken);

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendEmailConfirmationRequest>(request =>
                request.AccountId == result.Value &&
                request.Email == "admin@example.com" &&
                request.FirstName == "Admin" &&
                request.LastName == "User"),
            cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnDuplicateEmail_WhenAccountAlreadyExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var existingAccount = Account.Create(
            Guid.NewGuid(),
            new Email("ADMIN@EXAMPLE.COM")).Value;

        _accountRepositoryMock
            .GetByEmailAsync("ADMIN@EXAMPLE.COM", cancellationToken)
            .Returns(existingAccount);

        var handler = new RegisterAdminCommandHandler(
            _accountRepositoryMock,
            _roleRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock,
            _userProfileClientMock,
            _publishEndpointMock,
            NullLogger<RegisterAdminCommandHandler>.Instance);

        // Act
        Result<Guid> result = await handler.Handle(
            new RegisterAdminCommand("admin@example.com", "password-123", "Admin", "User"),
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
            Arg.Any<ApplicationRoles>(),
            Arg.Any<CancellationToken>());
    }
}
