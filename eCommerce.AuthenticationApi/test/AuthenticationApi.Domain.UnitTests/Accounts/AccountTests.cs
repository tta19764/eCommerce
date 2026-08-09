using AuthenticationApi.Domain.Accounts;
using FluentAssertions;
using Xunit;

namespace AuthenticationApi.Domain.UnitTests.Accounts;

public sealed class AccountTests
{
    [Fact]
    public void Create_ShouldInitializeActiveUnconfirmedAccount()
    {
        var id = Guid.NewGuid();

        var result = Account.Create(id, new Email("user@example.com"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.IsActive.Should().BeTrue();
        result.Value.IsEmailConfirmed.Should().BeFalse();
        result.Value.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldFail_WhenIdIsEmpty()
    {
        var result = Account.Create(Guid.Empty, new Email("user@example.com"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InvalidId);
    }

    [Fact]
    public void SetIdentityId_ShouldRejectWhitespace()
    {
        var account = CreateAccount();

        var result = account.SetIdentityId("  ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.EmptyIdentityId);
    }

    [Fact]
    public void SetUserId_ShouldRejectEmptyIdentifier()
    {
        var account = CreateAccount();

        var result = account.SetUserId(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.EmptyUserId);
    }

    [Fact]
    public void ConfirmEmail_ShouldBeIdempotent()
    {
        var account = CreateAccount();
        var firstConfirmation = DateTime.UtcNow;

        account.ConfirmEmail(firstConfirmation);
        account.ConfirmEmail(firstConfirmation.AddHours(1));

        account.EmailConfirmedAtUtc.Should().Be(firstConfirmation);
    }

    [Fact]
    public void Delete_ShouldDeactivateAccountAndPreventRepeatedDeletion()
    {
        var account = CreateAccount();
        var deletedAtUtc = DateTime.UtcNow;

        var firstResult = account.Delete(deletedAtUtc);
        var secondResult = account.Delete(deletedAtUtc.AddMinutes(1));

        firstResult.IsSuccess.Should().BeTrue();
        account.IsActive.Should().BeFalse();
        account.DeletedAtUtc.Should().Be(deletedAtUtc);
        secondResult.IsFailure.Should().BeTrue();
        secondResult.Error.Should().Be(AccountErrors.NotActive);
    }

    [Fact]
    public void ConfirmEmail_ShouldFail_WhenAccountIsInactive()
    {
        var account = CreateAccount();
        account.Delete(DateTime.UtcNow);

        var result = account.ConfirmEmail(DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.NotActive);
    }

    private static Account CreateAccount()
    {
        return Account.Create(Guid.NewGuid(), new Email("user@example.com")).Value;
    }
}
