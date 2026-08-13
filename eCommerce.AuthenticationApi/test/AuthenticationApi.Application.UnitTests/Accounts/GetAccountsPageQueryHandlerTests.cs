using AuthenticationApi.Application.Accounts.GetAccounts;
using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using SharedLibrary.Application.Abstractions.Caching;
using UserApi.Messages.Users;
using Xunit;

namespace AuthenticationApi.Application.UnitTests.Accounts;

public sealed class GetAccountsPageQueryHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock = Substitute.For<IAccountRepository>();
    private readonly IRequestClient<GetUserDetailsRequest> _userDetailsClientMock =
        Substitute.For<IRequestClient<GetUserDetailsRequest>>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();

    [Fact]
    public async Task Handle_ShouldTrackAccountPageCacheKey()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var query = new GetAccountsPageQuery(2, 25);

        _accountRepositoryMock
            .GetPageAsync(2, 25, cancellationToken)
            .Returns([]);
        _accountRepositoryMock
            .CountAsync(cancellationToken)
            .Returns(0);

        var handler = new GetAccountsPageQueryHandler(
            _accountRepositoryMock,
            _userDetailsClientMock,
            _cacheServiceMock);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _cacheServiceMock.Received(1).SetAsync(
            "auth:accounts:page-keys",
            Arg.Is<List<string>>(keys =>
                keys.Count == 1 && keys[0] == query.CacheKey),
            TimeSpan.FromDays(1),
            cancellationToken);
    }
}
