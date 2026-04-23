using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Moq;
using Castle.Components.DictionaryAdapter;

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

    [Fact]
    public async Task UpdateUser_NotFound_ReturnsFail()
    {
        //Arrange
        var updateDto = new UpdateUserDto {Email = "Email@test.com", Name = "Name Test", Password = "Test"};
        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        //Act
        var result = await _userService.UpdateUserAsync(Guid.NewGuid(), updateDto);

        //Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateUser_EmailAlreadyExists_ReturnsFail()
    {
        //Arrange 
        var updateDto = new UpdateUserDto {Email = "Email@test.com", Name = "Name Test", Password = "Test"};
        var user = new User("Name Test2", "Email2@test.com", "HashTest");
        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);
        _userRepositoryMock
            .Setup(u => u.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        //Act
        var result = await _userService.UpdateUserAsync(Guid.NewGuid(), updateDto);

        //Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.EmailAlreadyExists", result.Error.Code);
    }

    [Fact]
    public async Task UpdateUser_ShouldNotCheckEmailUniqueness_WhenEmailIsTheSame()
    {
        //Arrange
        var updateDto = new UpdateUserDto {Name = "Name Test", Email = "email@test.com", Password = "Test"};
        var user = new User("Name Test2", "email@test.com", "HashTest");
        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(u => u.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);

        //Act
        var result = await _userService.UpdateUserAsync(user.Id, updateDto);

        //Assert
        Assert.True(result.Success);
        _userRepositoryMock.Verify(
            u => u.ExistsByEmailAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateUser_ValidInput_CallsRepositoryUpdateAsync()
    {
        //Arrange
        var updateDto = new UpdateUserDto {Name = "Name Test", Email = "email@test.com", Password = "Test"};
        var user = new User("Name Test2", "email2@test.com", "HashTest");

        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(u => u.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(true);
        
        //Act
        var result = await _userService.UpdateUserAsync(user.Id, updateDto);

        //Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.NotEqual("HashTest", user.PasswordHash);
        Assert.Equal(updateDto.Name, result.Value.Name);
        Assert.Equal(updateDto.Email, result.Value.Email);
        _userRepositoryMock.Verify(
            u => u.UpdateAsync(It.IsAny<User>()),
            Times.Once
        );
    }
}