using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Moq;


namespace Taskly.Tests;

public class TeamServiceTests
{
    private readonly Mock<ITeamRepository> _teamRepositoryMock;
    private readonly TeamService _teamService;
    private readonly Mock<IUserService> _userServiceMock;

    public TeamServiceTests()
    {
        _teamRepositoryMock = new Mock<ITeamRepository>();
        _userServiceMock = new Mock<IUserService>();
        _teamService = new TeamService(_teamRepositoryMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task AddTeam_InvalidName_ReturnFail()
    {
        // Arrange
        var teamDto = new CreateTeamDto { Name = "" };

        // Act
        var result = await _teamService.AddTeamAsync(teamDto, Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.InvalidName", result.Error.Code);
    }

    [Fact]
    public async Task AddTeam_ValidInput_CallsRepositoryAddAsync()
    {
        // Arrange
        var teamDto = new CreateTeamDto { Name = "Team Test" };
        var ownerId = Guid.NewGuid();

        // Act
        await _teamService.AddTeamAsync(teamDto, ownerId);
        
        // Assert
        _teamRepositoryMock.Verify(
        r => r.AddAsync(It.Is<Team>(t =>
            t.Name == teamDto.Name &&
            t.OwnerId == ownerId &&
            t.UserIds.Contains(ownerId))),
        Times.Once);
    }

    [Fact]
    public async Task AddTeam_ValidInput_AssignsOwnerAndAddsOwnerToMembers()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var dto = new CreateTeamDto { Name = "Team Test" };

        // Act
        var result = await _teamService.AddTeamAsync(dto, ownerId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        Assert.Equal(ownerId, result.Value.OwnerId);
        Assert.Contains(ownerId, result.Value.UserIds);
    }

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
        Assert.Equal("Team.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task AddMember_TeamInactive_ReturnsFail()
    {
        // Arrange
        var team = new Team("Team Test", Guid.NewGuid());
        team.Update(null, false);

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                            .ReturnsAsync(team);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task AddMember_UserNotFound_ReturnsFail()
    {
        // Arrange
        var team = new Team("Team Test", Guid.NewGuid());

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(team);
        _userServiceMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("User.NotFound", result.Error.Code);

    }

    [Fact]
    public async Task AddMember_UserAlreadyMember_ReturnsFail()
    {
        // Arrange
        var user = new User("User Test", "test@test.com", "Hash");
        var team = new Team("Team Test", Guid.NewGuid());

        team.UserIds.Add(user.Id);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id))
            .ReturnsAsync(team);

        _userServiceMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _teamService.AddMemberAsync(team.Id, user.Id);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.UserAlreadyMember", result.Error.Code);
    }

    [Fact]
    public async Task AddMember_ValidInput_CallsUpdateAndReturnsOk()
    {
        // Arrange
        var team = new Team("Team Test", Guid.NewGuid());
        var user = new User("User Test", "Test@Test.com", "Test");

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(team.Id)).ReturnsAsync(team);
        _teamRepositoryMock.Setup(r => r.AddMemberAsync(team.Id, user.Id)).ReturnsAsync(true);
        _userServiceMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _teamService.AddMemberAsync(team.Id, user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal(team.Id, result.Value.TeamId);

        _teamRepositoryMock.Verify(r => r.AddMemberAsync(team.Id, user.Id), Times.Once);

    }

    [Fact]
    public async Task RemoveMember_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(teamId))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _teamService.RemoveMemberAsync(teamId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task RemoveMember_TeamInactive_ReturnsFail()
    {
        // Arrange
        var team = new Team("Team Test", Guid.NewGuid());
        team.Update(null, false);

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                            .ReturnsAsync(team);

        // Act
        var result = await _teamService.RemoveMemberAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task RemoveMember_UserNotMember_ReturnsFail()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var team = new Team("Team Test", Guid.NewGuid());

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        // Act
        var result = await _teamService.RemoveMemberAsync(teamId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.UserNotMember", result.Error.Code);
    }

    [Fact]
    public async Task RemoveMember_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var team = new Team("Team Test", Guid.NewGuid());
        team.UserIds.Add(userId);

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(teamId))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(t => t.RemoveMemberAsync(teamId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _teamService.RemoveMemberAsync(teamId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        Assert.Equal(teamId, result.Value.TeamId);
        Assert.Equal(userId, result.Value.UserId);

        _teamRepositoryMock.Verify(
            t => t.RemoveMemberAsync(teamId, userId),
            Times.Once);
    }

}
