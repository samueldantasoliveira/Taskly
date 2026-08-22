using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Domain.Exceptions;

namespace Taskly.Tests;


public class TeamTests{
    [Fact]
    public void AddMember_ShouldAddUser_WhenUserIsNotMember()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var team = new Team("Test team", Guid.NewGuid());

        // Act
        team.AddMember(userId);

        // Assert
        Assert.Contains(userId, team.UserIds);
    }

    [Fact]
    public void AddMember_ShouldThrowUserAlreadyMemberException_WhenUserIsAlreadyMember()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var team = new Team("Test team", Guid.NewGuid());
        team.AddMember(userId);

        // Act & Assert
        Assert.Throws<UserAlreadyMemberException>(() => team.AddMember(userId));
    }
    
    [Fact]
    public void RemoveMember_ShouldRemoveMember_WhenUserIsMember()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var team = new Team("Test team", Guid.NewGuid());
        team.AddMember(userId);

        // Act 
        team.RemoveMember(userId);

        // Assert
        Assert.DoesNotContain(userId, team.UserIds);
    }

    [Fact]
    public void RemoveMember_ShouldThrowUserNotMemberException_WhenUserIsNotMember()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var team = new Team("Test team", Guid.NewGuid());

        // Act & Assert
        Assert.Throws<UserNotMemberException>(() => team.RemoveMember(userId));

    }

    [Fact]
    public void RemoveMember_ShouldThrowOwnerCannotBeRemovedException_WhenUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var team = new Team("Test team", userId);

        // Act & Assert
        Assert.Throws<OwnerCannotBeRemovedException>(() => team.RemoveMember(userId));
    }
}

