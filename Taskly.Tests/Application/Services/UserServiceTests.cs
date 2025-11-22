using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Moq;

namespace Taskly.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task AddUser_EmailAlreadyExists_ReturnsFail()
    {
        // Arrange
        var userDto = new CreateUserDto { Name = "User Test", Password = "Password", Email = "Test@Test.com"};
        _userRepositoryMock.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        var result = await _userService.AddUserAsync(userDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.EmailAlreadyExists", result.Error.Code);
    }
    
    [Fact]
    public async Task AddUser_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        var userDto = new CreateUserDto { Name = "User Test", Password = "Password", Email = "Test@Test.com"};

        // Act
        var result = await _userService.AddUserAsync(userDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(userDto.Name, result.Value.Name);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Name == userDto.Name)),
            Times.Once
        );
    }
}