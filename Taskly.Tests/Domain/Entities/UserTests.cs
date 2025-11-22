using Taskly.Domain.Entities;

namespace Taskly.Tests;

public class UserTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateUser_InvalidName_ThrowsException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new User(invalidName, "Email@Test.com", "PasswordTest"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Email.com")]
    [InlineData("Teste")]
    public void CreateUser_InvalidEmail_ThrowsException(string invalidEmail)
    {
        Assert.Throws<ArgumentException>(() => new User("NameTest", invalidEmail, "PasswordTest"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateUser_InvalidPassword_ThrowsException(string invalidPassword)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new User("NameTest", "Email@Test.com", invalidPassword));
    }

}