using SharedLibrary.Domain.Abstractions;

namespace UserApi.Domain.Users;

/// <summary>
/// Domain errors produced by user operations.
/// </summary>
public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User was not found");
    public static readonly Error EmptyFirstName = new("User.EmptyFirstName", "First name is required");
    public static readonly Error EmptyLastName = new("User.EmptyLastName", "Last name is required");
    public static readonly Error EmptyEmail = new("User.EmptyEmail", "Email is required");
}
