using SharedLibrary.Domain.Abstractions;

namespace UserApi.Domain.Users;

/// <summary>
/// User profile aggregate root.
/// </summary>
public sealed class User : Entity
{
    private User()
    {
        FirstName = null!;
        LastName = null!;
        Email = null!;
    }

    private User(Guid id, FirstName firstName, LastName lastName, Email email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    /// <summary>
    /// User first name.
    /// </summary>
    public FirstName FirstName { get; private set; }

    /// <summary>
    /// User last name.
    /// </summary>
    public LastName LastName { get; private set; }

    /// <summary>
    /// User email address.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Optional image asset identifier used by profile UIs.
    /// </summary>
    public Guid? ImageId { get; private set; }

    /// <summary>
    /// Displayable full name composed from first and last name.
    /// </summary>
    public string FullName => $"{FirstName.Value} {LastName.Value}";

    /// <summary>
    /// Creates a user profile when required fields are provided.
    /// </summary>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="email">The user's email address captured from authentication registration.</param>
    /// <returns>The created user, or a validation failure.</returns>
    public static Result<User> Create(FirstName firstName, LastName lastName, Email email)
    {
        if (string.IsNullOrWhiteSpace(firstName.Value))
        {
            return Result.Failure<User>(UserErrors.EmptyFirstName);
        }

        if (string.IsNullOrWhiteSpace(lastName.Value))
        {
            return Result.Failure<User>(UserErrors.EmptyLastName);
        }

        if (string.IsNullOrWhiteSpace(email.Value))
        {
            return Result.Failure<User>(UserErrors.EmptyEmail);
        }

        return new User(Guid.NewGuid(), firstName, lastName, email);
    }

    /// <summary>
    /// Updates profile details when at least one supplied value changes.
    /// </summary>
    /// <param name="firstName">The optional replacement first name.</param>
    /// <param name="lastName">The optional replacement last name.</param>
    /// <param name="imageId">The optional image asset identifier.</param>
    /// <returns>A success result, or a validation failure.</returns>
    public Result Update(FirstName? firstName, LastName? lastName, Guid? imageId)
    {
        if (firstName is not null && string.IsNullOrWhiteSpace(firstName.Value))
        {
            return Result.Failure(UserErrors.EmptyFirstName);
        }

        if (lastName is not null && string.IsNullOrWhiteSpace(lastName.Value))
        {
            return Result.Failure(UserErrors.EmptyLastName);
        }

        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        ImageId = imageId;

        return Result.Success();
    }
}
