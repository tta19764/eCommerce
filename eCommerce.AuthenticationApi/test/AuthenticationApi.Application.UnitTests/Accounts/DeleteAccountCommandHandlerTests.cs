using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.DeleteAccount;
using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;
using Xunit;

namespace AuthenticationApi.Application.UnitTests.Accounts;

public sealed class DeleteAccountCommandHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IIdentityProvider _identityProviderMock = Substitute.For<IIdentityProvider>();
    private readonly IRequestClient<DeleteUserProfileRequest> _userProfileClientMock =
        Substitute.For<IRequestClient<DeleteUserProfileRequest>>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();

    [Fact]
    public async Task Handle_ShouldInvalidateAccountPagesAfterDelete()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var account = Account.Create(
            Guid.NewGuid(),
            new Email("USER@EXAMPLE.COM")).Value;
        account.SetIdentityId("identity-subject");
        account.SetUserId(userId);

        _accountRepositoryMock
            .GetByIdAsync(account.Id, cancellationToken)
            .Returns(account);
        _userProfileClientMock
            .GetResponse<DeleteUserProfileResponse>(
                new DeleteUserProfileRequest(userId),
                cancellationToken)
            .Returns(new TestResponse<DeleteUserProfileResponse>(
                new DeleteUserProfileResponse(true, null, null)));
        _identityProviderMock
            .DeleteAsync(account.IdentityId, cancellationToken)
            .Returns(Result.Success());

        var handler = new DeleteAccountCommandHandler(
            _accountRepositoryMock,
            _unitOfWorkMock,
            _identityProviderMock,
            _userProfileClientMock,
            _cacheServiceMock,
            NullLogger<DeleteAccountCommandHandler>.Instance);

        // Act
        var result = await handler.Handle(
            new DeleteAccountCommand(account.Id),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _accountRepositoryMock.Received(1).Delete(account);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
        await _cacheServiceMock.Received(1).RemoveAsync(
            "auth:accounts:page-keys",
            cancellationToken);
    }
}
