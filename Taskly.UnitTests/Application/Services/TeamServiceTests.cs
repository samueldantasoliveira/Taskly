using Taskly.Domain.Entities;
using Taskly.Application;
using Taskly.Application.DTOs;
using Moq;
using Taskly.Application.Results;


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
    public async Task UpdateTeam_TeamNotFound_ReturnsFail()
    {
        // Arrange
        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _teamService.UpdateTeamAsync(
            Guid.NewGuid(),
            new UpdateTeamDto(),
            Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateTeam_UserIsNotOwner_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team("Team Test", ownerId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Act
        var result = await _teamService.UpdateTeamAsync(
            team.Id,
            new UpdateTeamDto(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotOwner, result.Error);
    }

    [Fact]
    public async Task UpdateTeam_UpdateFails_ReturnsNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = new Team("Team Test", ownerId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(r => r.UpdateAsync(team))
            .ReturnsAsync(false);

        // Act
        var result = await _teamService.UpdateTeamAsync(
            team.Id,
            new UpdateTeamDto { Name = "New Name" },
            ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task UpdateTeam_ValidInput_UpdatesTeam()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        var team = new Team("Old Name", ownerId);

        var dto = new UpdateTeamDto
        {
            Name = "New Name",
            IsActive = false
        };

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(r => r.UpdateAsync(team))
            .ReturnsAsync(true);

        // Act
        var result = await _teamService.UpdateTeamAsync(
            team.Id,
            dto,
            ownerId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("New Name", result.Value!.Name);
        Assert.False(result.Value.IsActive);

        _teamRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Team>(t =>
                t.Name == "New Name" &&
                t.IsActive == false)),
            Times.Once);
    }

    [Fact]
    public async Task AddMember_TeamNotFound_ReturnsFail()
    {
        // Arrange
        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Team?)null);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

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

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(team);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task AddMember_UserNotFound_ReturnsFail()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var team = new Team("Team Test", authenticatedUserId);

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(team);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _teamService.AddMemberAsync(Guid.NewGuid(), Guid.NewGuid(), authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.UserNotFound", result.Error.Code);

    }

    [Fact]
    public async Task AddMember_UserIsNotOwner_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team("Team Test", ownerId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Act
        var result = await _teamService.AddMemberAsync(
            team.Id,
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotOwner, result.Error);
    }

    [Fact]
    public async Task AddMember_UpdateFails_ReturnsNotFound()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var team = new Team("Team Test", authenticatedUserId);
        var user = new User("User Test", "test@test.com", "Test");

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teamRepositoryMock
            .Setup(r => r.UpdateAsync(team))
            .ReturnsAsync(false);

        // Act
        var result = await _teamService.AddMemberAsync(
            team.Id,
            user.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task AddMember_ValidInput_CallsUpdateAndReturnsOk()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var team = new Team("Team Test", authenticatedUserId);
        var user = new User("User Test", "Test@Test.com", "Test");

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);
        _teamRepositoryMock.Setup(r => r.UpdateAsync(team)).ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _teamService.AddMemberAsync(team.Id, user.Id, authenticatedUserId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal(team.Id, result.Value.TeamId);

        _teamRepositoryMock.Verify(r => r.UpdateAsync(team), Times.Once);

    }

    [Fact]
    public async Task RemoveMember_TeamNotFound_ReturnsFail()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _teamRepositoryMock
            .Setup(t => t.GetByIdAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _teamService.RemoveMemberAsync(teamId, userId, Guid.NewGuid());

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

        _teamRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(team);

        // Act
        var result = await _teamService.RemoveMemberAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal("Team.Inactive", result.Error.Code);
    }

    [Fact]
    public async Task RemoveMember_UserIsNotOwner_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team("Team Test", ownerId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Act
        var result = await _teamService.RemoveMemberAsync(
            team.Id,
            Guid.NewGuid(),
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotOwner, result.Error);
    }

    [Fact]
    public async Task RemoveMember_UpdateFails_ReturnsNotFound()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var team = new Team("Team Test", authenticatedUserId);
        team.AddMember(userId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(r => r.UpdateAsync(team))
            .ReturnsAsync(false);

        // Act
        var result = await _teamService.RemoveMemberAsync(
            team.Id,
            userId,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task RemoveMember_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var authenticatedUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var team = new Team("Team Test", authenticatedUserId);
        team.AddMember(userId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(r => r.UpdateAsync(team))
            .ReturnsAsync(true);

        // Act
        var result = await _teamService.RemoveMemberAsync(
            team.Id,
            userId,
            authenticatedUserId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(team.Id, result.Value.TeamId);
        Assert.Equal(userId, result.Value.UserId);

        _teamRepositoryMock.Verify(
            r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _teamRepositoryMock.Verify(
            r => r.UpdateAsync(team),
            Times.Once);
    }

    [Fact]
    public async Task DeleteTeam_TeamNotFound_ReturnsFail()
    {
        // Arrange
        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _teamService.DeleteTeamAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteTeam_UserIsNotOwner_ReturnsFail()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team("Team Test", ownerId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        // Act
        var result = await _teamService.DeleteTeamAsync(
            team.Id,
            authenticatedUserId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotOwner, result.Error);
    }

    [Fact]
    public async Task DeleteTeam_DeleteFails_ReturnsNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = new Team("Team Test", ownerId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(r => r.DeleteAsync(team.Id))
            .ReturnsAsync(false);

        // Act
        var result = await _teamService.DeleteTeamAsync(
            team.Id,
            ownerId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task DeleteTeam_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var team = new Team("Team Test", ownerId);

        _teamRepositoryMock
            .Setup(r => r.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(r => r.DeleteAsync(team.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _teamService.DeleteTeamAsync(
            team.Id,
            ownerId);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);

        _teamRepositoryMock.Verify(
            r => r.DeleteAsync(team.Id),
            Times.Once);
    }

    [Fact]
public async Task LeaveTeam_TeamNotFound_ReturnsFail()
{
    // Arrange
    var teamId = Guid.NewGuid();
    var authenticatedUserId = Guid.NewGuid();

    _teamRepositoryMock
        .Setup(repo => repo.GetByIdAsync(teamId, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Team?)null);

    // Act
    var result = await _teamService.LeaveTeamAsync(
        teamId,
        authenticatedUserId
    );

    // Assert
    Assert.False(result.Success);
    Assert.Equal(TeamErrors.NotFound, result.Error);

    _teamRepositoryMock.Verify(
        repo => repo.UpdateAsync(It.IsAny<Team>()),
        Times.Never
    );
}


[Fact]
public async Task LeaveTeam_TeamInactive_ReturnsFail()
{
    // Arrange
    var authenticatedUserId = Guid.NewGuid();

    var team = new Team(
        "Test team",
        ownerId: Guid.NewGuid()
    );
    team.Update(null, false);

    _teamRepositoryMock
        .Setup(repo => repo.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(team);

    // Act
    var result = await _teamService.LeaveTeamAsync(
        team.Id,
        authenticatedUserId
    );

    // Assert
    Assert.False(result.Success);
    Assert.Equal(TeamErrors.Inactive, result.Error);

    _teamRepositoryMock.Verify(
        repo => repo.UpdateAsync(It.IsAny<Team>()),
        Times.Never
    );
}


[Fact]
public async Task LeaveTeam_ValidMember_RemovesMemberAndReturnsSuccess()
{
    // Arrange
    var ownerId = Guid.NewGuid();
    var authenticatedUserId = Guid.NewGuid();

    var team = new Team(
        "Test team",
        ownerId: ownerId
    );

    team.AddMember(authenticatedUserId);

    _teamRepositoryMock
        .Setup(repo => repo.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(team);

    _teamRepositoryMock
        .Setup(repo => repo.UpdateAsync(team))
        .ReturnsAsync(true);

    // Act
    var result = await _teamService.LeaveTeamAsync(
        team.Id,
        authenticatedUserId
    );

    // Assert
    Assert.True(result.Success);
    Assert.DoesNotContain(authenticatedUserId, team.UserIds);

    _teamRepositoryMock.Verify(
        repo => repo.UpdateAsync(team),
        Times.Once
    );
}


[Fact]
public async Task LeaveTeam_UpdateFails_ReturnsFail()
{
    // Arrange
    var ownerId = Guid.NewGuid();
    var authenticatedUserId = Guid.NewGuid();

    var team = new Team(
        "Test team",
        ownerId: ownerId
    );

    team.AddMember(authenticatedUserId);

    _teamRepositoryMock
        .Setup(repo => repo.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(team);

    _teamRepositoryMock
        .Setup(repo => repo.UpdateAsync(team))
        .ReturnsAsync(false);

    // Act
    var result = await _teamService.LeaveTeamAsync(
        team.Id,
        authenticatedUserId
    );

    // Assert
    Assert.False(result.Success);
    Assert.Equal(TeamErrors.NotFound, result.Error);

    _teamRepositoryMock.Verify(
        repo => repo.UpdateAsync(team),
        Times.Once
    );
}


    [Fact]
    public async Task LeaveTeam_ValidMember_UpdatesTeamWithoutAuthenticatedUser()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var authenticatedUserId = Guid.NewGuid();

        var team = new Team(
            "Test team",
            ownerId: ownerId
        );

        team.AddMember(authenticatedUserId);

        _teamRepositoryMock
            .Setup(repo => repo.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        _teamRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Team>()))
            .ReturnsAsync(true);

        // Act
        var result = await _teamService.LeaveTeamAsync(
            team.Id,
            authenticatedUserId
        );

        // Assert
        Assert.True(result.Success);

        _teamRepositoryMock.Verify(
            repo => repo.UpdateAsync(
                It.Is<Team>(updatedTeam =>
                    !updatedTeam.UserIds.Contains(authenticatedUserId)
                )
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetUserTeams_UserNotFound_ReturnsFail()
    {
        var userId = Guid.NewGuid();

        // Act
        var result = await _teamService.GetUserTeamsAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Equal(TeamErrors.UserNotFound, result.Error);

        _teamRepositoryMock.Verify(
            repository => repository.GetUserTeamsAsync(It.IsAny<Guid>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetUserTeams_UserHasTeams_ReturnsMappedTeams()
    {
        // Arrange
        var user = new User(
            "Test user",
            "Test@email.com",
            "TestPassword"
        );
        var userId = user.Id;

        var team = new Team(
            "Test team",
            user.Id
        );

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teamRepositoryMock
            .Setup(repository => repository.GetUserTeamsAsync(userId))
            .ReturnsAsync(new List<Team>
            {
                team
            });

        // Act
        var result = await _teamService.GetUserTeamsAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        var response = Assert.Single(result.Value);

        Assert.Equal(team.Id, response.Id);
        Assert.Equal(team.Name, response.Name);
        Assert.Equal(team.IsActive, response.IsActive);
        Assert.Equal(team.OwnerId, response.OwnerId);
        Assert.Equal(team.UserIds, response.UserIds);
    }

    [Fact]
    public async Task GetUserTeams_UserHasNoTeams_ReturnsEmptyList()
    {
        // Arrange
        var user = new User(
            "Test user",
            "Test@email.com",
            "TestPassword"
        );
        var userId = user.Id;


        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _teamRepositoryMock
            .Setup(repository => repository.GetUserTeamsAsync(userId))
            .ReturnsAsync(new List<Team>());

        // Act
        var result = await _teamService.GetUserTeamsAsync(userId);

        // Assert
        Assert.True(result.Success);

        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);

        _teamRepositoryMock.Verify(
            repository => repository.GetUserTeamsAsync(userId),
            Times.Once
        );
    }

    [Fact]
    public async Task GetById_TeamNotFound_ReturnsFail()
    {
        var teamId = Guid.NewGuid();

        _teamRepositoryMock
            .Setup(repository => repository.GetByIdAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var result = await _teamService.GetByIdAsync(teamId, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal(TeamErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetById_UserNotMember_ReturnsNotAuthorized()
    {
        var team = new Team("Test team", Guid.NewGuid());

        _teamRepositoryMock
            .Setup(repository => repository.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var result = await _teamService.GetByIdAsync(team.Id, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal(TeamErrors.NotAuthorized, result.Error);
    }

    [Fact]
    public async Task GetById_UserIsMember_ReturnsMappedTeam()
    {
        var ownerId = Guid.NewGuid();
        var team = new Team("Test team", ownerId);

        _teamRepositoryMock
            .Setup(repository => repository.GetByIdAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);

        var result = await _teamService.GetByIdAsync(team.Id, ownerId);

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(team.Id, result.Value.Id);
        Assert.Equal(team.Name, result.Value.Name);
        Assert.Equal(team.IsActive, result.Value.IsActive);
        Assert.Equal(team.OwnerId, result.Value.OwnerId);
        Assert.Equal(team.UserIds, result.Value.UserIds);
    }

}
