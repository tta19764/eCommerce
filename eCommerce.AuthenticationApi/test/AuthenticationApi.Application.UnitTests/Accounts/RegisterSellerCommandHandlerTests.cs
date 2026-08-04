using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.RegisterSeller;
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

public class RegisterSellerCommandHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IRoleRepository _roleRepositoryMock = Substitute.For<IRoleRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIdentityProvider _identityProviderMock = Substitute.For<IIdentityProvider>();
    private readonly IRequestClient<CreateUserProfileRequest> _userProfileClientMock =
        Substitute.For<IRequestClient<CreateUserProfileRequest>>();
    private readonly IPublishEndpoint _publishEndpointMock = Substitute.For<IPublishEndpoint>();

    [Fact]
    public async Task Handle_Should_RegisterIdentityAndCreateSellerAccount()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var sellerRole = Role.Seller;
        const string identityId = "keycloak-seller-user-id";

        _roleRepositoryMock.GetByNameAsync(ApplicationRoles.Seller, cancellationToken).Returns(sellerRole);
        _identityProviderMock
            .RegisterAsync(
                Arg.Any<Guid>(),
                "seller@example.com",
                "password-123",
                "Seller",
                "User",
                ApplicationRoles.Seller,
                cancellationToken)
            .Returns(Result.Success(identityId));

        _userProfileClientMock
            .GetResponse<CreateUserProfileResponse>(
                Arg.Any<CreateUserProfileRequest>(),
                cancellationToken)
            .Returns(new TestResponse<CreateUserProfileResponse>(
                new CreateUserProfileResponse(Guid.Parse("33333333-3333-3333-3333-333333333333"), true, null, null)));

        var handler = new RegisterSellerCommandHandler(
            _accountRepositoryMock,
            _roleRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock,
            _userProfileClientMock,
            _publishEndpointMock,
            NullLogger<RegisterSellerCommandHandler>.Instance);

        var command = new RegisterSellerCommand(
            "  seller@example.com  ",
            "password-123",
            "Seller",
            "User");

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _identityProviderMock.Received(1).RegisterAsync(
            result.Value,
            "seller@example.com",
            "password-123",
            "Seller",
            "User",
            ApplicationRoles.Seller,
            cancellationToken);

        _accountRepositoryMock.Received(1).Add(Arg.Is<Account>(account =>
            account.Id == result.Value &&
            account.Email.Value == "SELLER@EXAMPLE.COM" &&
            account.IdentityId == identityId &&
            account.UserId == Guid.Parse("33333333-3333-3333-3333-333333333333") &&
            account.Roles.Any(accountRole => accountRole.RoleId == sellerRole.Id)));

        await _unitOfWorkMock.Received(2).SaveChangesAsync(cancellationToken);

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<SendEmailConfirmationRequest>(request =>
                request.AccountId == result.Value &&
                request.Email == "seller@example.com" &&
                request.FirstName == "Seller" &&
                request.LastName == "User"),
            cancellationToken);
    }
}
