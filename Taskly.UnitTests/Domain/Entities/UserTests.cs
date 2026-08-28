using Taskly.Domain.Entities;
using Taskly.Domain.Exceptions;

namespace Taskly.Tests;

public class UserTests
{
    [Fact]
    public void CreateUser_EmailWithUppercase_NormalizesEmail()
    {
        var user = new User("Name Test", "Email@Test.COM", "PasswordTest");

        Assert.Equal("email@test.com", user.Email);
    }

    [Fact]
    public void UpdateUser_EmailWithUppercase_NormalizesEmail()
    {
        var user = new User("Name Test", "old@test.com", "PasswordTest");

        user.Update(null, "New@Test.COM", null);

        Assert.Equal("new@test.com", user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateUser_InvalidName_ThrowsException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<InvalidUserNameException>(() => new User(invalidName, "Email@Test.com", "PasswordTest"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Email.com")]
    [InlineData("Teste")]
    public void CreateUser_InvalidEmail_ThrowsException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<InvalidUserEmailException>(() => new User("NameTest", invalidEmail, "PasswordTest"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateUser_InvalidPassword_ThrowsException(string invalidPassword)
    {
        // Act & Assert
        Assert.Throws<InvalidUserPasswordException>(() => new User("NameTest", "Email@Test.com", invalidPassword));
    }

}
