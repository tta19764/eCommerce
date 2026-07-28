using FluentAssertions;
using UserApi.Domain.Users;
using Xunit;

namespace UserApi.Domain.UnitTests.Users;

public class UserTests
{
    [Fact]
    public void Create_Should_ReturnUser_WhenValuesAreValid()
    {
        // Act
        var result = User.Create(
            new FirstName("John"),
            new LastName("Smith"),
            new Email("john.smith@example.com"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.FirstName.Value.Should().Be("John");
        result.Value.LastName.Value.Should().Be("Smith");
        result.Value.Email.Value.Should().Be("john.smith@example.com");
        result.Value.FullName.Should().Be("John Smith");
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenFirstNameIsEmpty()
    {
        // Act
        var result = User.Create(
            new FirstName(" "),
            new LastName("Smith"),
            new Email("john.smith@example.com"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmptyFirstName);
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenLastNameIsEmpty()
    {
        // Act
        var result = User.Create(
            new FirstName("John"),
            new LastName(" "),
            new Email("john.smith@example.com"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmptyLastName);
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenEmailIsEmpty()
    {
        // Act
        var result = User.Create(
            new FirstName("John"),
            new LastName("Smith"),
            new Email(" "));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmptyEmail);
    }

    [Fact]
    public void Update_Should_ChangeProfileInfoOnly()
    {
        // Arrange
        var user = User.Create(
            new FirstName("John"),
            new LastName("Smith"),
            new Email("john.smith@example.com")).Value;

        // Act
        var imageId = Guid.NewGuid();

        var result = user.Update(new FirstName("Jane"), new LastName("Doe"), imageId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.FullName.Should().Be("Jane Doe");
        user.Email.Value.Should().Be("john.smith@example.com");
        user.ImageId.Should().Be(imageId);
    }
}
