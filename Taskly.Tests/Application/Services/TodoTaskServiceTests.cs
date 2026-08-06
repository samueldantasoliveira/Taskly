using Taskly.Application;
using Taskly.Domain.Entities;
using Moq;
using Taskly.Application.DTOs;
using Taskly.Domain;
using Taskly.Application.Results;

namespace Taskly.Tests;

public class TodoTaskServiceTests
{
    public readonly Mock<IProjectService> _projectServiceMock;
    public readonly TodoTaskService _todoTaskService;
    public readonly Mock<ITodoTaskRepository> _todoTaskRepositoryMock;
    public readonly Mock<IUserService> _userServiceMock;
    public readonly Mock<ITeamService> _teamServiceMock;
    public TodoTaskServiceTests()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _todoTaskRepositoryMock = new Mock<ITodoTaskRepository>();
        _userServiceMock = new Mock<IUserService>();
        _teamServiceMock = new Mock<ITeamService>();
        _todoTaskService = new TodoTaskService(
            _todoTaskRepositoryMock.Object,
            _projectServiceMock.Object,
            _userServiceMock.Object,
            _teamServiceMock.Object);
    }

    [Fact]
    public async Task AddTodoTask_ProjectNotFound_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "Title Test", Description = "Description Test", ProjectId = Guid.NewGuid() };
        _projectServiceMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync((Project?)null);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectNotFound, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_ProjectInactive_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "Title Test", Description = "Description Test", ProjectId = Guid.NewGuid() };
        Project project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Inactive, Guid.NewGuid());
        _projectServiceMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);

        // Act 
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, Guid.NewGuid());

        //Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectInactive, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_UserNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "Title Test", Description = "Description Test", ProjectId = Guid.NewGuid(), AssignedUserId = Guid.NewGuid() };
        Project project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active, Guid.NewGuid());
        Team team = new Team("Team Test", authenticatedUserId);
        team.UserIds.Add(createTodoTaskDto.AssignedUserId.Value);

        _projectServiceMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);
        _teamServiceMock.Setup(t => t.GetByIdAsync(project.TeamId))
                                .ReturnsAsync(team);
        _userServiceMock.Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync((User?)null);

        // Act 
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.UserNotFound, result.Error);

    }

    [Fact]
    public async Task AddTodoTask_InvalidTitle_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "", Description = "Description Test", ProjectId = Guid.NewGuid(), AssignedUserId = Guid.NewGuid() };

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TodoTask.InvalidTitle", result.Error.Code);
    }

    [Fact]
    public async Task AddTodoTask_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        User user = new User("User Test", "Test@Test.com", "Test");
        Team team = new Team("Team Test", authenticatedUserId);
        team.UserIds.Add(user.Id);
        Project project = new Project("Project Test", "Description Test", team.Id, ProjectStatus.Active, Guid.NewGuid());
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto { Title = "TodoTask Test", Description = "Description Test", ProjectId = project.Id, AssignedUserId = user.Id };


        _projectServiceMock.Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(project);
        _teamServiceMock.Setup(t => t.GetByIdAsync(project.TeamId))
                                .ReturnsAsync(team);
        _userServiceMock.Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
                                .ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, authenticatedUserId);

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
    public async Task AddTodoTask_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active, Guid.NewGuid());
        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id
        };

        _projectServiceMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.TeamNotFound, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_TeamInactive_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var team = new Team("Team Test", authenticatedUserId);
        team.Update(null, false);

        var project = new Project("Project Test", "Description Test", team.Id, ProjectStatus.Active, Guid.NewGuid());
        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id
        };

        _projectServiceMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.TeamInactive, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_AuthenticatedUserNotTeamMember_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var team = new Team("Team Test", Guid.NewGuid());
        var project = new Project("Project Test", "Description Test", team.Id, ProjectStatus.Active, Guid.NewGuid());
        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id
        };

        _projectServiceMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.UserNotTeamMember, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_AssignedUserNotTeamMember_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();
        var team = new Team("Team Test", authenticatedUserId);
        var project = new Project("Project Test", "Description Test", team.Id, ProjectStatus.Active, Guid.NewGuid());
        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id,
            AssignedUserId = assignedUserId
        };

        _projectServiceMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(createTodoTaskDto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.AssignedUserNotTeamMember, result.Error);
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
        _projectServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Project?)null);

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
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Inactive, Guid.NewGuid());
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test" };
        var todoTask = new TodoTask("TodoTask Title", "Description Test", project.Id, Guid.NewGuid());

        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _projectServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);

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
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active, Guid.NewGuid());
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test", AssignedUserId = Guid.NewGuid() };
        var todoTask = new TodoTask("TodoTask Title", "Description Test", project.Id, Guid.NewGuid());

        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _projectServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
        _userServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act
        var result = await _todoTaskService.UpdateAsync(Guid.NewGuid(), updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_NoChanges_ReturnsFail()
    {
        // Arrange
        var user = new User("User Test", "Test@Test.com", "Test");
        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active, Guid.NewGuid());
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test", AssignedUserId = user.Id };
        var todoTask = new TodoTask("Title Test", "Description Test", project.Id, user.Id);


        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _todoTaskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoTask>())).ReturnsAsync(false);
        _projectServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
        _userServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

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
            user = new User("User Test", "Test@Test.com", "Test");

        var project = new Project("Project Test", "Description Test", Guid.NewGuid(), ProjectStatus.Active, Guid.NewGuid());
        var updateDto = new UpdateTodoTaskDto { Title = "Title Test", Description = "Desctiption Test", AssignedUserId = userId };
        var todoTask = new TodoTask("Title Test 2", "Description Test", project.Id, userId);


        _todoTaskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(todoTask);
        _todoTaskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoTask>())).ReturnsAsync(true);
        _projectServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(project);
        _userServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

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
