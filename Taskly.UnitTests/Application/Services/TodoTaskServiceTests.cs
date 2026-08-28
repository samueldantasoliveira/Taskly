using Taskly.Application;
using Taskly.Domain.Entities;
using Moq;
using Taskly.Application.DTOs;
using Taskly.Domain;
using Taskly.Application.Results;

namespace Taskly.Tests;

public class TodoTaskServiceTests
{
    public readonly Mock<IProjectRepository> _projectRepositoryMock;
    public readonly TodoTaskService _todoTaskService;
    public readonly Mock<ITodoTaskRepository> _todoTaskRepositoryMock;
    public readonly Mock<IUserRepository> _userRepositoryMock;
    public readonly Mock<ITeamRepository> _teamRepositoryMock;

    public TodoTaskServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _todoTaskRepositoryMock = new Mock<ITodoTaskRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();

        _todoTaskService = new TodoTaskService(
            _todoTaskRepositoryMock.Object,
            _projectRepositoryMock.Object,
            _userRepositoryMock.Object,
            _teamRepositoryMock.Object);
    }

    [Fact]
    public async Task AddTodoTask_ProjectNotFound_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test",
            ProjectId = Guid.NewGuid()
        };

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectNotFound, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_ProjectInactive_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test",
            ProjectId = Guid.NewGuid()
        };

        Project project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Inactive,
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        // Act 
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectInactive, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_UserNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test",
            ProjectId = Guid.NewGuid(),
            AssignedUserId = Guid.NewGuid()
        };

        Project project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        Team team = new Team("Team Test", authenticatedUserId);
        team.AddMember(createTodoTaskDto.AssignedUserId.Value);

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        // Act 
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task AddTodoTask_InvalidTitle_ReturnsFail()
    {
        // Arrange
        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "",
            Description = "Description Test",
            ProjectId = Guid.NewGuid(),
            AssignedUserId = Guid.NewGuid()
        };

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            Guid.NewGuid());

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

        User user = new User(
            "User Test",
            "Test@Test.com",
            "Test");

        Team team = new Team(
            "Team Test",
            authenticatedUserId);

        team.AddMember(user.Id);

        Project project = new Project(
            "Project Test",
            "Description Test",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        CreateTodoTaskDto createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id,
            AssignedUserId = user.Id
        };

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            authenticatedUserId);

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

        var project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id
        };

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            authenticatedUserId);

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

        var team = new Team(
            "Team Test",
            authenticatedUserId);

        team.Update(null, false);

        var project = new Project(
            "Project Test",
            "Description Test",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id
        };

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            authenticatedUserId);

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

        var team = new Team(
            "Team Test",
            Guid.NewGuid());

        var project = new Project(
            "Project Test",
            "Description Test",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id
        };

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            authenticatedUserId);

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

        var team = new Team(
            "Team Test",
            authenticatedUserId);

        var project = new Project(
            "Project Test",
            "Description Test",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        var createTodoTaskDto = new CreateTodoTaskDto
        {
            Title = "TodoTask Test",
            Description = "Description Test",
            ProjectId = project.Id,
            AssignedUserId = assignedUserId
        };

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AddTodoTaskAsync(
            createTodoTaskDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.AssignedUserNotTeamMember, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test"
        };

        // Act
        var result = await _todoTaskService.UpdateAsync(
            Guid.NewGuid(),
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ProjectNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test"
        };

        var todoTask = new TodoTask(
            "TodoTask Test",
            "Description Test",
            Guid.NewGuid(),
            Guid.NewGuid());

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(todoTask);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            Guid.NewGuid(),
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectNotFound, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ProjectInactive_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Inactive,
            Guid.NewGuid());

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Desctiption Test"
        };

        var todoTask = new TodoTask(
            "TodoTask Title",
            "Description Test",
            project.Id,
            Guid.NewGuid());

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(todoTask);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            Guid.NewGuid(),
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectInactive, result.Error);
    }


    [Fact]
    public async Task UpdateAsync_AuthenticatedUserNotTeamMember_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team(
            "Team Test",
            Guid.NewGuid());

        var project = new Project(
            "Project Test",
            "Description Test",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        var todoTask = new TodoTask(
            "Task",
            "Description",
            project.Id,
            null);

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Desctiption Test"
        };

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            todoTask.Id,
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.UserNotTeamMember, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        var todoTask = new TodoTask(
            "Task",
            "Description",
            project.Id,
            null);

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test"
        };

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(project.TeamId))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            todoTask.Id,
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.TeamNotFound, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_TeamInactive_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team(
            "Team Test",
            authenticatedUserId);

        team.Update(null, false);

        var project = new Project(
            "Project Test",
            "Description Test",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        var todoTask = new TodoTask(
            "Task",
            "Description",
            project.Id,
            null);

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Desctiption Test"
        };

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(project.TeamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            todoTask.Id,
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.TeamInactive, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_NoChanges_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team(
            "Team Test",
            authenticatedUserId);

        var user = new User(
            "User Test",
            "Test@Test.com",
            "Test");

        team.AddMember(user.Id);

        var project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test"
        };

        var todoTask = new TodoTask(
            "Title Test",
            "Description Test",
            project.Id,
            user.Id);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(false);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            todoTask.Id,
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NoChangesDetected, result.Error);
    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task UpdateAsync_ValidInput_CallsRepositoryUpdateAsync(
        string userIdString)
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        Guid? userId = Guid.Parse(userIdString);
        User? user = null;

        var team = new Team(
            "Team Test",
            authenticatedUserId);

        if (userId != Guid.Empty)
        {
            user = new User(
                "User Test",
                "Test@Test.com",
                "Test");

            team.AddMember(user.Id);
        }

        var project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test"
        };

        var todoTask = new TodoTask(
            "Title Test 2",
            "Description Test",
            project.Id,
            userId);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(true);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            todoTask.Id,
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(updateDto.Title, result.Value.Title);
        Assert.Equal(updateDto.Description, result.Value.Description);
        
        _todoTaskRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<TodoTask>(t =>
                t.Title == updateDto.Title &&
                t.Description == updateDto.Description
            )),
            Times.Once);
    }

    [Fact]
    public async Task StartTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        // Act
        var result = await _todoTaskService.StartTaskAsync(
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task StartTaskAsync_ShouldReturnNotAssignedUser_WhenTaskHasNoAssignedUser()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            null);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        // Act
        var result = await _todoTaskService.StartTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotAssignedUser, result.Error);
    }

    [Fact]
    public async Task StartTaskAsync_ShouldReturnNotAssignedUser_WhenAuthenticatedUserIsNotAssignedToTask()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid());

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        // Act
        var result = await _todoTaskService.StartTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotAssignedUser, result.Error);
    }

    [Fact]
    public async Task StartTaskAsync_ShouldReturnSuccess_WhenAuthenticatedUserIsAssignedToTask()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            authenticatedUserId);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(true);

        // Act
        var result = await _todoTaskService.StartTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.True(result.Success);

        _todoTaskRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<TodoTask>(t =>
                t.Id == todoTask.Id &&
                t.Status == TodoStatus.InProgress)),
            Times.Once);
    }

    [Fact]
    public async Task StartTaskAsync_ShouldReturnNoChangesDetected_WhenTaskUpdateFails()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            authenticatedUserId);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(false);

        // Act
        var result = await _todoTaskService.StartTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NoChangesDetected, result.Error);
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        // Act
        var result = await _todoTaskService.CompleteTaskAsync(
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldReturnNotAssignedUser_WhenTaskHasNoAssignedUser()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            null);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        // Act
        var result = await _todoTaskService.CompleteTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotAssignedUser, result.Error);
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldReturnNotAssignedUser_WhenAuthenticatedUserIsNotAssignedToTask()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid());

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        // Act
        var result = await _todoTaskService.CompleteTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotAssignedUser, result.Error);
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldReturnSuccess_WhenAuthenticatedUserIsAssignedToTask()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            authenticatedUserId);

        todoTask.Start();

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(true);

        // Act
        var result = await _todoTaskService.CompleteTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.True(result.Success);

        _todoTaskRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<TodoTask>(t =>
                t.Id == todoTask.Id &&
                t.Status == TodoStatus.Done)),
            Times.Once);
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldReturnNoChangesDetected_WhenTaskUpdateFails()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            authenticatedUserId);

        todoTask.Start();

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(false);

        // Act
        var result = await _todoTaskService.CompleteTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NoChangesDetected, result.Error);
    }

    [Fact]
    public async Task CancelTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        // Act
        var result = await _todoTaskService.CancelTaskAsync(
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task CancelTaskAsync_ShouldReturnNotAssignedUser_WhenTaskHasNoAssignedUser()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            null);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        // Act
        var result = await _todoTaskService.CancelTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotAssignedUser, result.Error);
    }

    [Fact]
    public async Task CancelTaskAsync_ShouldReturnNotAssignedUser_WhenAuthenticatedUserIsNotAssignedToTask()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid());

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        // Act
        var result = await _todoTaskService.CancelTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotAssignedUser, result.Error);
    }

    [Fact]
    public async Task CancelTaskAsync_ShouldReturnSuccess_WhenAuthenticatedUserIsAssignedToTask()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            authenticatedUserId);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(true);

        // Act
        var result = await _todoTaskService.CancelTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.True(result.Success);

        _todoTaskRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<TodoTask>(t =>
                t.Id == todoTask.Id &&
                t.Status == TodoStatus.Cancelled)),
            Times.Once);
    }

    [Fact]
    public async Task CancelTaskAsync_ShouldReturnNoChangesDetected_WhenTaskUpdateFails()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var todoTask = new TodoTask(
            "Task",
            "Description",
            Guid.NewGuid(),
            authenticatedUserId);

        _todoTaskRepositoryMock
            .Setup(r => r.GetByIdAsync(todoTask.Id))
            .ReturnsAsync(todoTask);

        _todoTaskRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(false);

        // Act
        var result = await _todoTaskService.CancelTaskAsync(
            todoTask.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NoChangesDetected, result.Error);
    }

   [Fact]
    public async Task AssignUserAsync_ShouldAssignUser_WhenUserIsValidAndBelongsToTeam()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        var team = new Team(
            "Test team",
            authenticatedUserId
        );
        
        var assignedUser = new User
        (
            "Test user",
            "Email@test.com",
            "123456"
        );

        var assignedUserId = assignedUser.Id;
        team.AddMember(assignedUserId);

        

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(assignedUserId))
            .ReturnsAsync(assignedUser);

        _todoTaskRepositoryMock
            .Setup(x => x.UpdateAsync(task))
            .ReturnsAsync(true);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            assignedUserId,
            authenticatedUserId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(assignedUserId, task.AssignedUserId);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(task),
            Times.Once);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldRemoveAssignedUser_WhenUserIdIsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            Guid.NewGuid()
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        var team = new Team(
            "Test team",
            authenticatedUserId
        );

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        _todoTaskRepositoryMock
            .Setup(x => x.UpdateAsync(task))
            .ReturnsAsync(true);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            null,
            authenticatedUserId);

        // Assert
        Assert.True(result.Success);
        Assert.Null(task.AssignedUserId);

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(task),
            Times.Once);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TodoTask?)null);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.NotFound, result.Error);

        _projectRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenProjectDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.ProjectNotFound, result.Error);

        _teamRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenProjectIsInactive()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Inactive,
            authenticatedUserId
        );

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.ProjectInactive, result.Error);

        _teamRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenTeamDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.TeamNotFound, result.Error);

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenTeamIsInactive()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        var team = new Team(
            "Test team",
            authenticatedUserId
        );

        team.Update("Test team", false);

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            assignedUserId,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.TeamInactive, result.Error);

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenAuthenticatedUserIsNotTeamMember()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        var team = new Team(
            "Test team",
            Guid.NewGuid()
        );

        team.AddMember(assignedUserId);

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            assignedUserId,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.UserNotTeamMember, result.Error);

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenAssignedUserDoesNotExist()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        var team = new Team(
            "Test team",
            authenticatedUserId
        );

        team.AddMember(assignedUserId);

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(assignedUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            assignedUserId,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.UserNotFound, result.Error);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenAssignedUserIsNotTeamMember()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var assignedUser = new User(
            "Test user",
            "email@test.com",
            "123456"
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        var team = new Team(
            "Test team",
            authenticatedUserId
        );

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(assignedUser.Id))
            .ReturnsAsync(assignedUser);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            assignedUser.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.AssignedUserNotTeamMember, result.Error);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignUserAsync_ShouldFail_WhenRepositoryUpdateFails()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var assignedUser = new User(
            "Test user",
            "email@test.com",
            "123456"
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        var project = new Project(
            "Test project",
            "Test description",
            teamId,
            ProjectStatus.Active,
            authenticatedUserId
        );

        var team = new Team(
            "Test team",
            authenticatedUserId
        );

        team.AddMember(assignedUser.Id);

        _todoTaskRepositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(task);

        _projectRepositoryMock
            .Setup(x => x.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        _teamRepositoryMock
            .Setup(x => x.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(assignedUser.Id))
            .ReturnsAsync(assignedUser);

        _todoTaskRepositoryMock
            .Setup(x => x.UpdateAsync(task))
            .ReturnsAsync(false);

        // Act
        var result = await _todoTaskService.AssignUserAsync(
            taskId,
            assignedUser.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.NoChangesDetected, result.Error);

        _todoTaskRepositoryMock.Verify(
            x => x.UpdateAsync(task),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _todoTaskService.DeleteTaskAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnProjectNotFound_WhenProjectDoesNotExist()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var task = new TodoTask(
            "Test task",
            "Test description",
            projectId,
            null
        );

        _todoTaskRepositoryMock.Setup(
            r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        // Act
        var result = await _todoTaskService.DeleteTaskAsync(task.Id, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectNotFound, result.Error);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnProjectInactive_WhenProjectIsInactive()
    {
        // Arrange
        var project = new Project(
            "Test project",
            "Test description",
            Guid.NewGuid(),
            ProjectStatus.Inactive,
            Guid.NewGuid()
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null
        );

        _todoTaskRepositoryMock.Setup(
            r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        _projectRepositoryMock.Setup(
            r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        // Act
        var result = await _todoTaskService.DeleteTaskAsync(task.Id, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.ProjectInactive, result.Error);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnTeamNotFound_WhenTeamDoesNotExist()
    {
        // Arrange
        var project = new Project(
            "Test project",
            "Test description",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid()
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null
        );

        _todoTaskRepositoryMock.Setup(
            r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        _projectRepositoryMock.Setup(
            r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        // Act
        var result = await _todoTaskService.DeleteTaskAsync(task.Id, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.TeamNotFound, result.Error);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnTeamInactive_WhenTeamIsInactive()
    {
        // Arrange
        var authenticatedUser = Guid.NewGuid();

        var team = new Team(
            "Test team", 
            Guid.NewGuid()
        );
        team.Update(null, false);

        var project = new Project(
            "Test project",
            "Test description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid()
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null
        );

        _todoTaskRepositoryMock.Setup(
            r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        _projectRepositoryMock.Setup(
            r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock.Setup(
            r => r.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.DeleteTaskAsync(task.Id, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.TeamInactive, result.Error);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnUserNotTeamMember_WhenAuthenticatedUserIsNotTeamMember()
    {
        // Arrange
        var authenticatedUser = Guid.NewGuid();

        var team = new Team(
            "Test team", 
            Guid.NewGuid()
        );

        var project = new Project(
            "Test project",
            "Test description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid()
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null
        );

        _todoTaskRepositoryMock.Setup(
            r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        _projectRepositoryMock.Setup(
            r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock.Setup(
            r => r.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.DeleteTaskAsync(task.Id, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.UserNotTeamMember, result.Error);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnNoChangesDetected_WhenRepositoryUpdateFails()
    {
        // Arrange
        var authenticatedUser = Guid.NewGuid();

        var team = new Team(
            "Test team", 
            authenticatedUser
        );
        
        var project = new Project(
            "Test project",
            "Test description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid()
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null
        );

        _todoTaskRepositoryMock.Setup(
            r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        _projectRepositoryMock.Setup(
            r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock.Setup(
            r => r.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        _todoTaskRepositoryMock.Setup(
            r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(false);

        // Act
        var result = await _todoTaskService.DeleteTaskAsync(task.Id, authenticatedUser);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.NoChangesDetected, result.Error);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldDeleteTask_WhenRequestIsValid()
    {
         // Arrange
        var authenticatedUser = Guid.NewGuid();

        var team = new Team(
            "Test team", 
            authenticatedUser
        );

        var project = new Project(
            "Test project",
            "Test description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid()
        );

        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null
        );

        _todoTaskRepositoryMock.Setup(
            r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        _projectRepositoryMock.Setup(
            r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamRepositoryMock.Setup(
            r => r.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        _todoTaskRepositoryMock.Setup(
            r => r.UpdateAsync(It.IsAny<TodoTask>()))
            .ReturnsAsync(true);

        // Act
        var result = await _todoTaskService.DeleteTaskAsync(task.Id, authenticatedUser);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(task.DeletedAt);

        _todoTaskRepositoryMock.Verify(
            r => r.UpdateAsync(task),
            Times.Once);
    }

    [Fact]
    public async Task GetById_TaskNotFound_ReturnsFail()
    {
        var result = await _todoTaskService.GetByIdAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetById_ProjectNotFound_ReturnsFail()
    {
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null);

        _todoTaskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var result = await _todoTaskService.GetByIdAsync(
            task.Id,
            Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.ProjectNotFound, result.Error);
    }

    [Fact]
    public async Task GetById_ProjectInactive_ReturnsFail()
    {
        var project = new Project(
            "Test project",
            "Test description",
            Guid.NewGuid(),
            ProjectStatus.Inactive,
            Guid.NewGuid());
        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null);

        _todoTaskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(task.Id))
            .ReturnsAsync(task);
        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        var result = await _todoTaskService.GetByIdAsync(
            task.Id,
            Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.ProjectInactive, result.Error);
    }

    [Fact]
    public async Task GetById_TeamNotFound_ReturnsFail()
    {
        var project = new Project(
            "Test project",
            "Test description",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());
        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null);

        _todoTaskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(task.Id))
            .ReturnsAsync(task);
        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        var result = await _todoTaskService.GetByIdAsync(
            task.Id,
            Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.TeamNotFound, result.Error);
    }

    [Fact]
    public async Task GetById_TeamInactive_ReturnsFail()
    {
        var authenticatedUserId = Guid.NewGuid();
        var team = new Team("Test team", authenticatedUserId);
        team.Update(null, false);
        var project = new Project(
            "Test project",
            "Test description",
            team.Id,
            ProjectStatus.Active,
            authenticatedUserId);
        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            authenticatedUserId);

        SetupGetByIdDependencies(task, project, team);

        var result = await _todoTaskService.GetByIdAsync(
            task.Id,
            authenticatedUserId);

        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.TeamInactive, result.Error);
    }

    [Fact]
    public async Task GetById_UserNotTeamMember_ReturnsFail()
    {
        var team = new Team("Test team", Guid.NewGuid());
        var project = new Project(
            "Test project",
            "Test description",
            team.Id,
            ProjectStatus.Active,
            team.OwnerId);
        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            null);

        SetupGetByIdDependencies(task, project, team);

        var result = await _todoTaskService.GetByIdAsync(
            task.Id,
            Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal(TodoTaskErrors.UserNotTeamMember, result.Error);
    }

    [Fact]
    public async Task GetById_ValidRequest_ReturnsMappedTask()
    {
        var authenticatedUserId = Guid.NewGuid();
        var team = new Team("Test team", authenticatedUserId);
        var project = new Project(
            "Test project",
            "Test description",
            team.Id,
            ProjectStatus.Active,
            authenticatedUserId);
        var task = new TodoTask(
            "Test task",
            "Test description",
            project.Id,
            authenticatedUserId);

        SetupGetByIdDependencies(task, project, team);

        var result = await _todoTaskService.GetByIdAsync(
            task.Id,
            authenticatedUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(task.Id, result.Value.Id);
        Assert.Equal(task.Title, result.Value.Title);
        Assert.Equal(task.Description, result.Value.Description);
        Assert.Equal(task.ProjectId, result.Value.ProjectId);
        Assert.Equal(task.AssignedUserId, result.Value.AssignedUserId);
        Assert.Equal(task.Status, result.Value.Status);
    }

    private void SetupGetByIdDependencies(
        TodoTask task,
        Project project,
        Team team)
    {
        _todoTaskRepositoryMock
            .Setup(repository => repository.GetByIdAsync(task.Id))
            .ReturnsAsync(task);
        _projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(project.Id))
            .ReturnsAsync(project);
        _teamRepositoryMock
            .Setup(repository => repository.GetByIdAsync(team.Id))
            .ReturnsAsync(team);
    }
}
