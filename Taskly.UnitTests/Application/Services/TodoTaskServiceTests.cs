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
        team.UserIds.Add(createTodoTaskDto.AssignedUserId.Value);

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

        team.UserIds.Add(user.Id);

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
    public async Task UpdateAsync_AssignedUserNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team(
            "Team Test",
            authenticatedUserId);

        var project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test",
            AssignedUserId = Guid.NewGuid()
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

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(team);

        // Act
        var result = await _todoTaskService.UpdateAsync(
            Guid.NewGuid(),
            updateDto,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TodoTaskErrors.UserNotFound, result.Error);
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
            Description = "Desctiption Test",
            AssignedUserId = Guid.NewGuid()
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
    public async Task UpdateAsync_AssignedUserNotTeamMember_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();

        var user = new User(
            "User Test",
            "Test@Test.com",
            "Test");

        var team = new Team(
            "Team Test",
            authenticatedUserId);

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

        var assignedUserId = Guid.NewGuid();

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Desctiption Test",
            AssignedUserId = assignedUserId
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
        Assert.Equal(TodoTaskErrors.AssignedUserNotTeamMember, result.Error);
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
            Description = "Description Test",
            AssignedUserId = Guid.NewGuid()
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
            Description = "Desctiption Test",
            AssignedUserId = Guid.NewGuid()
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

        team.UserIds.Add(user.Id);

        var project = new Project(
            "Project Test",
            "Description Test",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        var updateDto = new UpdateTodoTaskDto
        {
            Title = "Title Test",
            Description = "Description Test",
            AssignedUserId = user.Id
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

            team.UserIds.Add(user.Id);
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
            Description = "Description Test",
            AssignedUserId = userId
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
        Assert.Equal(updateDto.AssignedUserId, result.Value.AssignedUserId);

        _todoTaskRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<TodoTask>(t =>
                t.Title == updateDto.Title &&
                t.Description == updateDto.Description &&
                t.AssignedUserId == updateDto.AssignedUserId
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
}