using SharedLibrary.Domain.Abstractions;

namespace UserApi.Domain.Users;

/// <summary>
/// Domain errors produced by user operations.
/// </summary>
public static class UserErrors
{
    public static readonly Error InvalidId = new("User.InvalidId", "User id must not be empty");
    public static readonly Error AlreadyExists = new("User.AlreadyExists", "User already exists");
    public static readonly Error NotFound = new("User.NotFound", "User was not found");
    public static readonly Error EmptyFirstName = new("User.EmptyFirstName", "First name is required");
    public static readonly Error EmptyLastName = new("User.EmptyLastName", "Last name is required");
    public static readonly Error EmptyEmail = new("User.EmptyEmail", "Email is required");
    public static readonly Error EmptyIdentityId = new("User.EmptyIdentityId", "Identity id is required");
    public static readonly Error HasOrders = new("User.HasOrders", "User cannot be removed because orders exist for this user");
}
