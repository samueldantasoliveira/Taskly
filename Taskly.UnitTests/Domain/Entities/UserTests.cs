using Taskly.Domain.Entities;

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
        Assert.Throws<ArgumentException>(() => new User(invalidName, "Email@Test.com", "PasswordTest"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Email.com")]
    [InlineData("Teste")]
    public void CreateUser_InvalidEmail_ThrowsException(string invalidEmail)
    {
        // Act & Assert
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
