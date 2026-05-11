using Moq;
using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Domain;
using DnsClient.Protocol;

namespace Taskly.Tests;

public class ProjectServiceTests
{
    public readonly Mock<IProjectRepository> _projectRepositoryMock;
    public readonly Mock<ITeamService> _teamServiceMock;
    public readonly Mock<IUserService> _userServiceMock;
    public readonly ProjectService _projectService;
    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _teamServiceMock = new Mock<ITeamService>();
        _userServiceMock = new Mock<IUserService>();
        _projectService = new ProjectService(_projectRepositoryMock.Object, _teamServiceMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task AddProject_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test" };
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.TeamNotFound", result.Error.Code);
    }

    [Fact]
    public async Task AddProject_TeamInactive_ReturnsFail()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test", TeamId = Guid.NewGuid() };
        var team = new Team("Team Test");
        var ownerId = Guid.NewGuid();
        team.IsActive = false;

        _teamServiceMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.TeamInactive", result.Error.Code);
    }

    [Fact]
    public async Task AddProject_InvalidName_returnsFail()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "", Description = "Project Test", TeamId = Guid.NewGuid() };
        var team = new Team("Team Test");
        var ownerId = Guid.NewGuid();

        _teamServiceMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.InvalidName", result.Error.Code);
    }

    [Fact]
    public async Task AddProject_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        var team = new Team("Team Test");
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test", TeamId = team.Id };
        var ownerId = Guid.NewGuid();

        _teamServiceMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, ownerId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(projectDto.Name, result.Value.Name);
        Assert.Equal(projectDto.Description, result.Value.Description);
        Assert.Equal(projectDto.TeamId, result.Value.TeamId);

        _projectRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Project>(p =>
                p.Name == projectDto.Name &&
                p.Description == projectDto.Description &&
                p.TeamId == projectDto.TeamId &&
                p.Status == ProjectStatus.Active
            )),
        Times.Once
        );
    }
    [Fact]
    public async Task UpdateProject_ProjectNotFound_ReturnsFail()
    {
        // Arrange
        var dto = new UpdateProjectDto();

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateProject_OwnerNotFound_ReturnsFail()
    {
        // Arrange
        var dto = new UpdateProjectDto(){Name = "Test", OwnerId = Guid.NewGuid()};
        var project = new Project(
            "Project", 
            "Description", 
            Guid.NewGuid(), 
            ProjectStatus.Active, 
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);
        _userServiceMock
            .Setup(u => u.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.OwnerNotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateProject_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var dto = new UpdateProjectDto(){Name = "Test", TeamId = Guid.NewGuid()};
        var project = new Project(
            "Project", 
            "Description", 
            Guid.NewGuid(), 
            ProjectStatus.Active, 
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);
        
        _teamServiceMock
            .Setup(t => t.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.TeamNotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateProject_UpdateFails_ReturnsFail()
    {
        // Arrange
        var dto = new UpdateProjectDto
        {
            Name = "Test Name"
        };

        var project = new Project(
            "Project",
            "Description",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(p => p.UpdateAsync(It.IsAny<Project>()))
            .ReturnsAsync(false);

        // Act
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Project.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task UpdateProject_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var dto = new UpdateProjectDto
        {
            Name = "Test Name"
        };

        var project = new Project(
            "Project",
            "Description",
            Guid.NewGuid(),
            ProjectStatus.Active,
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(p => p.UpdateAsync(It.IsAny<Project>()))
            .ReturnsAsync(true);

        // Act
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        Assert.Equal(dto.Name, result.Value.Name);
        
        _projectRepositoryMock.Verify(
            p => p.UpdateAsync(It.IsAny<Project>()),
            Times.Once);
    }
}