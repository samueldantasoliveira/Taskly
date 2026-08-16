using Moq;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Domain.Entities;

namespace Taskly.Tests;

public class LoginServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginService _loginService;

    public LoginServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();

        _loginService = new LoginService(
            _userRepository.Object,
            _tokenServiceMock.Object
        );
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsInvalidCredentials()
    {
        // Arrange
        _userRepository
            .Setup(u => u.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _loginService.LoginAsync(
            "test@email.com",
            "123456"
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.InvalidCredentials", result.Error.Code);
    }


    [Fact]
    public async Task Login_InvalidPassword_ReturnsInvalidCredentials()
    {
        // Arrange
        var user = new User(
            "User Test",
            "test@email.com",
            PasswordHasher.HashPassword("correctPassword")
        );

        _userRepository
            .Setup(u => u.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        // Act
        var result = await _loginService.LoginAsync(
            "test@email.com",
            "wrongPassword"
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.InvalidCredentials", result.Error.Code);
    }


    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = new User(
            "User Test",
            "test@email.com",
            PasswordHasher.HashPassword("123456")
        );

        var expiresAt = DateTime.UtcNow.AddHours(1);

        _userRepository
            .Setup(u => u.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(t => t.GenerateToken(user, out expiresAt))
            .Returns("fake-token");

        // Act
        var result = await _loginService.LoginAsync(
            "test@email.com",
            "123456"
        );

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        Assert.Equal("fake-token", result.Value.Token);
        Assert.Equal(expiresAt, result.Value.ExpiresAt);

        Assert.Equal(user.Id, result.Value.User.Id);
        Assert.Equal(user.Name, result.Value.User.Name);
        Assert.Equal(user.Email, result.Value.User.Email);

        _tokenServiceMock.Verify(
            t => t.GenerateToken(user, out expiresAt),
            Times.Once
        );
    }


    [Fact]
    public async Task Login_EmailIsNormalizedBeforeSearch()
    {
        // Arrange
        var user = new User(
            "User Test",
            "test@email.com",
            PasswordHasher.HashPassword("123456")
        );

        _userRepository
            .Setup(u => u.GetByEmailAsync("test@email.com"))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(t => t.GenerateToken(user, out It.Ref<DateTime>.IsAny))
            .Returns("fake-token");

        // Act
        await _loginService.LoginAsync(
            "TEST@EMAIL.COM",
            "123456"
        );

        // Assert
        _userRepository.Verify(
            u => u.GetByEmailAsync("test@email.com"),
            Times.Once
        );
    }
}