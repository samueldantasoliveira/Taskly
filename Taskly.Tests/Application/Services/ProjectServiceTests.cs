using Moq;
using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Taskly.Domain;
using DnsClient.Protocol;
using Taskly.Application.Results;

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
        var authenticatedUserId = Guid.NewGuid();

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, authenticatedUserId);

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
        var team = new Team("Team Test", Guid.NewGuid());
        var authenticatedUserId = Guid.NewGuid();
        team.Update(null, false);

        _teamServiceMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, authenticatedUserId);

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
        var team = new Team("Team Test", Guid.NewGuid());
        var authenticatedUserId = Guid.NewGuid();

        _teamServiceMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.InvalidName", result.Error.Code);
    }

    [Fact]
    public async Task AddProject_UserNotMember_ReturnsFail()
    {
        // Arrange
        var team = new Team("Team Test", Guid.NewGuid());
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test", TeamId = team.Id };
        var authenticatedUserId = Guid.NewGuid();

        _teamServiceMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.UserNotTeamMember, result.Error);
    }

    [Fact]
    public async Task AddProject_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        var team = new Team("Team Test", Guid.NewGuid());
        var projectDto = new CreateProjectDto { Name = "Project Test", Description = "Project Test", TeamId = team.Id };
        var authenticatedUserId = Guid.NewGuid();

        team.UserIds.Add(authenticatedUserId);
        _teamServiceMock.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);

        // Act
        var result = await _projectService.AddProjectAsync(projectDto, authenticatedUserId);

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
        var authenticatedUserId = Guid.NewGuid();

        _projectRepositoryMock
            .Setup(p => p.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateProject_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
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
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Project.TeamNotFound", result.Error.Code);
    }

    [Fact]
    public async Task UpdateProject_UpdateFails_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var dto = new UpdateProjectDto
        {
            Name = "Test Name"
        };

        var team = new Team("Team", authenticatedUserId);


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
        
        _teamServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(team);

        // Act
        var result = await _projectService.UpdateProjectAsync(Guid.NewGuid(), dto, authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Project.NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task UpdateProject_NewTeamNotFound_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        var oldTeam = new Team("Old Team", ownerId);

        var project = new Project(
            "Project",
            "Description",
            oldTeam.Id,
            ProjectStatus.Active,
            ownerId);

        var newTeamId = Guid.NewGuid();

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(newTeamId))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _projectService.UpdateProjectAsync(
            project.Id,
            new UpdateProjectDto
            {
                TeamId = newTeamId
            },
            ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.TeamNotFound, result.Error);
    }

    [Fact]
    public async Task UpdateProject_NewTeamInactive_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        var oldTeam = new Team("Old Team", ownerId);

        var newTeam = new Team("New Team", ownerId);
        newTeam.Update(null, false);

        var project = new Project(
            "Project",
            "Description",
            oldTeam.Id,
            ProjectStatus.Active,
            ownerId);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(newTeam.Id))
            .ReturnsAsync(newTeam);

        // Act
        var result = await _projectService.UpdateProjectAsync(
            project.Id,
            new UpdateProjectDto
            {
                TeamId = newTeam.Id
            },
            ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.TeamInactive, result.Error);
    }

    [Fact]
    public async Task UpdateProject_UserNotMemberOfNewTeam_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        var oldTeam = new Team("Old Team", ownerId);

        var newTeam = new Team("New Team", Guid.NewGuid());

        var project = new Project(
            "Project",
            "Description",
            oldTeam.Id,
            ProjectStatus.Active,
            ownerId);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(newTeam.Id))
            .ReturnsAsync(newTeam);

        // Act
        var result = await _projectService.UpdateProjectAsync(
            project.Id,
            new UpdateProjectDto
            {
                TeamId = newTeam.Id
            },
            ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task UpdateProject_NotAuthorized_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team("Team Test", ownerId);

        var project = new Project(
            "Project",
            "Description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        // Act
        var result = await _projectService.UpdateProjectAsync(
            project.Id,
            new UpdateProjectDto(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task UpdateProject_TeamMemberNotAuthorized_ReturnsFail()
    {
        // Arrange
        var teamOwnerId = Guid.NewGuid();
        var projectOwnerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var team = new Team("Team Test", teamOwnerId);
        team.UserIds.Add(memberId);

        var project = new Project(
            "Project Test",
            "Description",
            team.Id,
            ProjectStatus.Active,
            projectOwnerId);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        // Act
        var result = await _projectService.UpdateProjectAsync(
            project.Id,
            new UpdateProjectDto(),
            memberId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task UpdateProject_TeamOwner_ReturnsSuccess()
    {
        // Arrange
        var teamOwnerId = Guid.NewGuid();

        var team = new Team("Team Test", teamOwnerId);

        var project = new Project(
            "Project",
            "Description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        var dto = new UpdateProjectDto
        {
            Name = "Updated Project"
        };

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        _projectRepositoryMock
            .Setup(r => r.UpdateAsync(project))
            .ReturnsAsync(true);

        // Act
        var result = await _projectService.UpdateProjectAsync(
            project.Id,
            dto,
            teamOwnerId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        _projectRepositoryMock.Verify(
            r => r.UpdateAsync(project),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProject_ProjectOwner_ReturnsSuccess()
    {
        // Arrange
        var teamOwnerId = Guid.NewGuid();
        var projectOwnerId = Guid.NewGuid();

        var team = new Team("Team Test", teamOwnerId);

        var project = new Project(
            "Project Test",
            "Description",
            team.Id,
            ProjectStatus.Active,
            projectOwnerId);

        var dto = new UpdateProjectDto
        {
            Name = "Updated Project"
        };

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(r => r.UpdateAsync(project))
            .ReturnsAsync(true);

        // Act
        var result = await _projectService.UpdateProjectAsync(
            project.Id,
            dto,
            projectOwnerId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        _projectRepositoryMock.Verify(
            r => r.UpdateAsync(project),
            Times.Once);

        _teamServiceMock.Verify(
            t => t.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteProject_NotAuthorized_ReturnsFail()
    {
        // Arrange
        var teamOwnerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team("Team Test", teamOwnerId);

        var project = new Project(
            "Project",
            "Description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        // Act
        var result = await _projectService.DeleteProjectAsync(
            project.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task DeleteProject_ProjectNotFound_ReturnsFail()
    {
        // Arrange
        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.DeleteProjectAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(ProjectErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteProject_TeamOwner_ReturnsSuccess()
    {
        // Arrange
        var teamOwnerId = Guid.NewGuid();

        var team = new Team("Team Test", teamOwnerId);

        var project = new Project(
            "Project",
            "Description",
            team.Id,
            ProjectStatus.Active,
            Guid.NewGuid());

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _teamServiceMock
            .Setup(t => t.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        _projectRepositoryMock
            .Setup(r => r.DeleteAsync(project.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _projectService.DeleteProjectAsync(
            project.Id,
            teamOwnerId);

        // Assert
        Assert.True(result.Success);

        _projectRepositoryMock.Verify(
            r => r.DeleteAsync(project.Id),
            Times.Once);
    }

    [Fact]
    public async Task DeleteProject_ProjectOwner_ReturnsSuccess()
    {
        // Arrange
        var teamOwnerId = Guid.NewGuid();
        var projectOwnerId = Guid.NewGuid();

        var team = new Team("Team Test", teamOwnerId);

        var project = new Project(
            "Project Test",
            "Description",
            team.Id,
            ProjectStatus.Active,
            projectOwnerId);

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(project.Id))
            .ReturnsAsync(project);

        _projectRepositoryMock
            .Setup(r => r.DeleteAsync(project.Id))
            .ReturnsAsync(true);

        // Não precisa buscar o time, pois o dono do projeto
        // já passa na primeira validação de CanManageProject.

        // Act
        var result = await _projectService.DeleteProjectAsync(
            project.Id,
            projectOwnerId);

        // Assert
        Assert.True(result.Success);

        _projectRepositoryMock.Verify(
            r => r.DeleteAsync(project.Id),
            Times.Once);

        _teamServiceMock.Verify(
            t => t.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }
}