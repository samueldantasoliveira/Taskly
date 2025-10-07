using Taskly.Application;
using Taskly.Domain.Entities;
using Moq;
using Taskly.Application.DTOs;
using Taskly.Domain;

namespace Taskly.Tests;

public class TodoTaskServiceTests
{
    public readonly Mock<IProjectRepository> _projectRepositoryMock;
    public readonly TodoTaskService _todoTaskService;
    public readonly Mock<ITodoTaskRepository> _todoTaskRepositoryMock;
    public readonly Mock<IUserRepository> _userRepositoryMock;
    public TodoTaskServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _todoTaskRepositoryMock = new Mock<ITodoTaskRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _todoTaskService = new TodoTaskService(_todoTaskRepositoryMock.Object, _projectRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task AddTodoTask_ProjectNotFound_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "Title Test", Description = "Description Test", ProjectId = Guid.NewGuid() };
        _projectRepositoryMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync((Project?)null);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task AddTodoTask_ProjectInactive_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "Title Test", Description = "Description Test", ProjectId = Guid.NewGuid() };
        Project project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Inactive);
        _projectRepositoryMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);

        // Act 
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto);

        //Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task AddTodoTask_UserNotFound_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "Title Test", Description = "Description Test", ProjectId = Guid.NewGuid(), AssignedUserId = Guid.NewGuid() };
        Project project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        _projectRepositoryMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);
        _userRepositoryMock.Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync((User?)null);

        // Act 
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.NotFound", result.Error.Code);

    }

    [Fact]
    public async Task AddTodoTask_UserInactive_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "Title Test", Description = "Description Test", ProjectId = Guid.NewGuid(), AssignedUserId = Guid.NewGuid() };
        Project project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        User user = new User("User Test") { IsActive = false };

        _projectRepositoryMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);
        _userRepositoryMock.Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task AddTodoTask_InvalidTitle_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "", Description = "Description Test", ProjectId = Guid.NewGuid(), AssignedUserId = Guid.NewGuid() };
        Project project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        User user = new User("User Test");

        _projectRepositoryMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);
        _userRepositoryMock.Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TodoTask.InvalidTitle", result.Error.Code);
    }

    [Fact]
    public async Task AddTodoTask_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        User user = new User("User Test");
        Project project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "TodoTask Test", Description = "Description Test", ProjectId = project.Id, AssignedUserId = user.Id };


        _projectRepositoryMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);
        _userRepositoryMock.Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(createTodoTaskDto.Title, result.Value.Title);
        Assert.Equal(createTodoTaskDto.Description, result.Value.Description);
        Assert.Equal(createTodoTaskDto.ProjectId, result.Value.ProjectId);
        Assert.Equal(createTodoTaskDto.AssignedUserId, result.Value.AssignedUserId);

        _todoTaskRepositoryMock.Verify(
            r => r.AddAsync(It.Is<TodoTask>(t =>
                t.Title == createTodoTaskDto.Title &&
                t.Description == createTodoTaskDto.Description &&
                t.ProjectId == createTodoTaskDto.ProjectId &&
                t.AssignedUserId == createTodoTaskDto.AssignedUserId
            )),
        Times.Once
        );
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFail()
    {
        // Arrange
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Description Test" };

        // Act
        var result = await _todoTaskService.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TodoTask.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_ProjectNotFound_ReturnsFail()
    {
        // Arrange
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Description Test" };
        var todoTask = new TodoTask("TodoTask Test", "Description Test", Guid.NewGuid(), Guid.NewGuid());

        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Project?)null);

        // Act
        var result = await _todoTaskService.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_ProjectInactive_ReturnsFail()
    {
        // Arrange
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Inactive);
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test" };
        var todoTask = new TodoTask("TodoTask Title", "Description Test", project.Id, Guid.NewGuid());

        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);

        // Act
        var result = await _todoTaskService.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_AssignedUserNotFound_ReturnsFail()
    {
        // Arrange
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test", AssignedUserId = Guid.NewGuid() };
        var todoTask = new TodoTask("TodoTask Title", "Description Test", project.Id, Guid.NewGuid());

        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act
        var result = await _todoTaskService.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_AssignedUserInactive_ReturnsFail()
    {
        // Arrange
        var user = new User("User Test") { IsActive = false };
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test", AssignedUserId = user.Id };
        var todoTask = new TodoTask("TodoTask Title", "Description Test", project.Id, Guid.NewGuid());


        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_NoChanges_ReturnsFail()
    {
        // Arrange
        var user = new User("User Test");
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test", AssignedUserId = user.Id };
        var todoTask = new TodoTask("Title Test", "Description Test", project.Id, user.Id);


        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _todoTaskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoTask>())).ReturnsAsync(false);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.UpdateAsync(todoTask.Id, updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TodoTask.NoChangesDetected", result.Error.Code);

    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111")] // with assigned User
    [InlineData("00000000-0000-0000-0000-000000000000")] // without assigned User
    public async Task UpdateAsync_ValidInput_CallsRepositoryUpdateAsync(string userIdString)
    {
        // Arrange
        Guid? userId = Guid.Parse(userIdString);
        User? user = null;
        if (userId != Guid.Empty)
            user = new User("User Test");

        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active);
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test", AssignedUserId = userId };
        var todoTask = new TodoTask("Title Test 2", "Description Test", project.Id, userId);


        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _todoTaskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoTask>())).ReturnsAsync(true);
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.UpdateAsync(todoTask.Id, updateDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(updateDto.Title, result.Value.Title);
        Assert.Equal(updateDto.Description, result.Value.Description);
        Assert.Equal(updateDto.AssignedUserId, result.Value.AssignedUserId);

        _todoTaskRepositoryMock.Verify(r => r.UpdateAsync(It.Is<TodoTask>(t =>
            t.Title == updateDto.Title &&
            t.Description == updateDto.Description &&
            t.AssignedUserId == updateDto.AssignedUserId
        )), Times.Once);
    }
}