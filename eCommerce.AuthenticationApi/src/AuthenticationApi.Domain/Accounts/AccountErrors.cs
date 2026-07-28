using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Domain errors produced by authentication account operations.
/// </summary>
public static class AccountErrors
{
    public static readonly Error InvalidId = new("Account.InvalidId", "Account id must not be empty");
    public static readonly Error EmptyEmail = new("Account.EmptyEmail", "Email is required");
    public static readonly Error EmptyIdentityId = new("Account.EmptyIdentityId", "Identity id is required");
    public static readonly Error DuplicateEmail = new("Account.DuplicateEmail", "An account with this email already exists");
    public static readonly Error InvalidCredentials = new("Account.InvalidCredentials", "Invalid email or password");
    public static readonly Error NotFound = new("Account.NotFound", "Account was not found");
    public static readonly Error NotActive = new("Account.NotActive", "Account is not active");
    public static readonly Error IdentityRegistrationFailed = new("Account.IdentityRegistrationFailed", "Identity registration failed");
    public static readonly Error IdentityDeletionFailed = new("Account.IdentityDeletionFailed", "Identity deletion failed");
    public static readonly Error ProfileCreationFailed = new("Account.ProfileCreationFailed", "User profile could not be created");
    public static readonly Error ProfileDeletionFailed = new("Account.ProfileDeletionFailed", "User profile could not be deleted");
}
