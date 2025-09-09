using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Moq;


namespace Taskly.Tests;

public class TeamServiceTests
{
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly TeamService _teamService;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public TeamServiceTests()
    {
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _teamService = new TeamService(_teamRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task AddTeam_WithEmptyName_ReturnFail()
    {
        // Arrange
        var teamDto = new CreateTeamDto { Name = "" };

        // Act
        var result = await _teamService.AddTeamAsync(teamDto);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("InvalidName", result.Error.Code);
    }

    [Fact]
    public async Task AddTeam_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        var teamDto = new CreateTeamDto { Name = "Team Test" };

        // Act
        await _teamService.AddTeamAsync(teamDto);
        // Assert
        _teamRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Team>(t => t.Name == teamDto.Name)),
            Times.Once
        );
    }

    //AddMemberAsync(Guid teamId, Guid userId)
    [Fact]
    public async Task AddMember_TeamNotFound_ReturnsFail()
    {
        // Arrange
        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                            .ReturnsAsync((Team?)null);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TeamNotFound", result.Error.Code);
    }

    [Fact]
    public async Task AddMember_TeamInvalid_ReturnsFail()
    {
        // Arrange
        var team = new Team("Team Test");
        team.IsActive = false;

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                            .ReturnsAsync(team);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("TeamInactive", result.Error.Code);
    }

    [Fact]
    public async Task AddMember_UserNotFound_ReturnsFail()
    {
        // Arrange
        var team = new Team("Team Test");

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("UserNotFound", result.Error.Code);

    }

    [Fact]
    public async Task AddMember_UserInvalid_ReturnsFail()
    {
        // Arrange
        var team = new Team("Team Test");
        var user = new User("User Test");
        user.IsActive = false;

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("UserInactive", result.Error.Code);
    }

    [Fact]
    public async Task AddMember_ValidInput_CallsUpdateAndReturnsOk()
    {
        // Arrange
        var team = new Team("Team Test");
        var user = new User("User Test");

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _teamService.AddMemberAsync(team.Id, user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal(team.Id, result.Value.TeamId);

        _teamRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Team>(t => t.UserIds.Contains(user.Id))), Times.Once);

    }

}
