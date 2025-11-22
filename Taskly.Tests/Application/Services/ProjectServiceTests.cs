using Moq;
using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Domain;

namespace Taskly.Tests;

public class ProjectServiceTests
{
    public readonly Mock<IProjectRepository> _projectRepositoryMock;
    public readonly Mock<ITeamRepository> _teamRepositoryMock;
    public readonly ProjectService _projectService;
    public ProjectServiceTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _projectService = new ProjectService(_projectRepositoryMock.Object, _teamRepositoryMock.Object);
    }

    [Fact]
    public async Task AddProject_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test" };

        // Act
        var result = await _projectService.AddProjectAsync(projectDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TeamNotFound", result.Error.Code);
    }

    [Fact]
    public async Task AddProject_TeamInactive_ReturnsFail()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test", TeamId = Guid.NewGuid() };
        var team = new Team("Team Test");
        team.IsActive = false;

        _teamRepositoryMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TeamInactive", result.Error.Code);
    }

    [Fact]
    public async Task AddProject_InvalidName_returnsFail()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "", Description = "Project Test", TeamId = Guid.NewGuid() };
        var team = new Team("Team Test");

        _teamRepositoryMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("InvalidName", result.Error.Code);
    }

    [Fact]
    public async Task AddProject_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        var team = new Team("Team Test");
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test", TeamId = team.Id };

        _teamRepositoryMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto);

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
}