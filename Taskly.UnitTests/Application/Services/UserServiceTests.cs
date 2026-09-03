using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Application.Results;
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
        _userRepositoryMock.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _userService.AddUserAsync(userDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.EmailAlreadyExists", result.Error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddUser_EmptyPassword_ReturnsInvalidPassword(
        string invalidPassword)
    {
        var userDto = new CreateUserDto
        {
            Name = "User Test",
            Email = "test@test.com",
            Password = invalidPassword
        };

        var result = await _userService.AddUserAsync(userDto);

        Assert.False(result.Success);
        Assert.Equal("User.InvalidPassword", result.Error?.Code);
        _userRepositoryMock.Verify(
            repository => repository.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _userRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
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
        Assert.Equal("test@test.com", result.Value.Email);

        _userRepositoryMock.Verify(
            repository => repository.ExistsByEmailAsync("test@test.com", It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            repository => repository.AddAsync(It.Is<User>(user =>
                user.Name == userDto.Name &&
                user.Email == "test@test.com"), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetByEmail_NormalizesEmailBeforeRepositoryLookup()
    {
        var user = new User("User Test", "test@test.com", "HashTest");
        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.GetByEmailAsync("TEST@TEST.COM");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("test@test.com", result.Email);
        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchByEmail_ValidEmail_ReturnsUserAndPropagatesCancellationToken()
    {
        var user = new User("User Test", "test@test.com", "HashTest");
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(
                "test@test.com",
                cancellationToken))
            .ReturnsAsync(user);

        var result = await _userService.SearchByEmailAsync(
            "  TEST@TEST.COM  ",
            cancellationToken);

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(user.Name, result.Value.Name);
        Assert.Equal(user.Email, result.Value.Email);
        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync(
                "test@test.com",
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public async Task SearchByEmail_InvalidEmail_ReturnsInvalidEmail(string? email)
    {
        var result = await _userService.SearchByEmailAsync(email);

        Assert.False(result.Success);
        Assert.Equal(UserErrors.InvalidEmail, result.Error);
        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchByEmail_UserNotFound_ReturnsNotFound()
    {
        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(
                "missing@test.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _userService.SearchByEmailAsync("missing@test.com");

        Assert.False(result.Success);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateUser_NotFound_ReturnsFail()
    {
        //Arrange
        var updateDto = new UpdateUserDto {Email = "Email@test.com", Name = "Name Test", Password = "Test"};
        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        //Act
        var result = await _userService.UpdateUserAsync(Guid.NewGuid(), updateDto);

        //Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateUser_EmptyPassword_ReturnsInvalidPassword(
        string invalidPassword)
    {
        var user = new User("User Test", "test@test.com", "HashTest");
        var updateDto = new UpdateUserDto { Password = invalidPassword };
        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _userService.UpdateUserAsync(user.Id, updateDto);

        Assert.False(result.Success);
        Assert.Equal("User.InvalidPassword", result.Error?.Code);
        _userRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUser_EmailAlreadyExists_ReturnsFail()
    {
        //Arrange 
        var updateDto = new UpdateUserDto {Email = "Email@test.com", Name = "Name Test", Password = "Test"};
        var user = new User("Name Test2", "Email2@test.com", "HashTest");
        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock
            .Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        var updateDto = new UpdateUserDto {Name = "Name Test", Email = "EMAIL@TEST.COM", Password = "Test"};
        var user = new User("Name Test2", "email@test.com", "HashTest");
        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(u => u.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        //Act
        var result = await _userService.UpdateUserAsync(user.Id, updateDto);

        //Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal("email@test.com", result.Value.Email);
        _userRepositoryMock.Verify(
            u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateUser_ValidInput_CallsRepositoryUpdateAsync()
    {
        //Arrange
        var updateDto = new UpdateUserDto {Name = "Name Test", Email = "EMAIL@TEST.COM", Password = "Test"};
        var user = new User("Name Test2", "email2@test.com", "HashTest");

        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(u => u.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        //Act
        var result = await _userService.UpdateUserAsync(user.Id, updateDto);

        //Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.NotEqual("HashTest", user.PasswordHash);
        Assert.Equal(updateDto.Name, result.Value.Name);
        Assert.Equal("email@test.com", result.Value.Email);
        _userRepositoryMock.Verify(
            repository => repository.ExistsByEmailAsync("email@test.com", It.IsAny<CancellationToken>()),
            Times.Once);
        _userRepositoryMock.Verify(
            u => u.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteUser_PropagatesCancellationTokenToRepository()
    {
        var userId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        _userRepositoryMock
            .Setup(repository => repository.DeleteAsync(userId, cancellationToken))
            .ReturnsAsync(true);

        var result = await _userService.DeleteUserAsync(userId, cancellationToken);

        Assert.True(result);
        _userRepositoryMock.Verify(
            repository => repository.DeleteAsync(userId, cancellationToken),
            Times.Once);
    }
}
