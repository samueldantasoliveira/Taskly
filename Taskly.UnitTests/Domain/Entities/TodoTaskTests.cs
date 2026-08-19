using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Domain.Exceptions;

namespace Taskly.Tests;


public class TodoTaskTests
{
    [Fact]
    public void Start_ShouldChangeStatusToInProgress_WhenTaskIsTodo()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );

        // Act
        task.Start();

        // Assert
        Assert.Equal(TodoStatus.InProgress, task.Status);

    }

    [Fact]
    public void Start_ShouldThrowInvalidTaskTransitionException_WhenTaskIsInProgress()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );

        task.Start();

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Start());

        // Assert
        Assert.Equal(TodoStatus.InProgress, task.Status);
    }

    [Fact]
    public void Start_ShouldThrowInvalidTaskTransitionException_WhenTaskIsDone()
    {
        // Arrange
        var task = new TodoTask("Title Test", "Description Test", Guid.NewGuid(), null);
        task.Start();
        task.Complete();

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Start());

        // Assert
        Assert.Equal(TodoStatus.Done, task.Status);
    }

    [Fact]
    public void Start_ShouldThrowInvalidTaskTransitionException_WhenTaskIsCancelled()
    {
        // Arrange
        var task = new TodoTask("Title Test", "Description Test", Guid.NewGuid(), null);
        task.Cancel();

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Start());

        // Assert
        Assert.Equal(TodoStatus.Cancelled, task.Status);
    }

    [Fact]
    public void Complete_ShouldChangeStatusToDone_WhenTaskIsInProgress()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );
        task.Start();

        // Act
        task.Complete();

        // Assert
        Assert.Equal(TodoStatus.Done, task.Status);
    }

    [Fact]
    public void Complete_ShouldThrowInvalidTaskTransitionException_WhenTaskIsTodo()
    {
        // Arrange 
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Complete());

        // Assert
        Assert.Equal(TodoStatus.Todo, task.Status);
    }

    [Fact]
    public void Complete_ShouldThrowInvalidTaskTransitionException_WhenTaskIsDone()
    {
        // Arrange 
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );
        task.Start();
        task.Complete();

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Complete());

        // Assert
        Assert.Equal(TodoStatus.Done, task.Status);
    }

    [Fact]
    public void Complete_ShouldThrowInvalidTaskTransitionException_WhenTaskIsCancelled()
    {
        // Arrange 
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );
        task.Cancel();

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Complete());

         // Assert
        Assert.Equal(TodoStatus.Cancelled, task.Status);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled_WhenTaskIsTodo()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );

        // Act
        task.Cancel();

        // Assert
        Assert.Equal(TodoStatus.Cancelled, task.Status);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled_WhenTaskIsInProgress()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );

        task.Start();

        // Act
        task.Cancel();

        // Assert
        Assert.Equal(TodoStatus.Cancelled, task.Status);
    }
    
    [Fact]
    public void Cancel_ShouldThrowInvalidTaskTransitionException_WhenTaskIsDone()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );

        task.Start();
        task.Complete();

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Cancel());

         // Assert
        Assert.Equal(TodoStatus.Done, task.Status);
    }

    [Fact]
    public void Cancel_ShouldThrowInvalidTaskTransitionException_WhenTaskIsCancelled()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );


        task.Cancel();

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Cancel());

         // Assert
        Assert.Equal(TodoStatus.Cancelled, task.Status);
    }

    [Fact]
    public void AssignUser_ShouldAssignUser_WhenTaskHasNoAssignedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            null
        );

        // Act
        task.AssignUser(userId);

        // Assert
        Assert.Equal(userId, task.AssignedUserId);
    }

    [Fact]
    public void AssignUser_ShouldChangeUser_WhenTaskAlreadyHasAssignedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        task.AssignUser(userId);

        // Assert
        Assert.Equal(userId, task.AssignedUserId);
    }

    [Fact]
    public void AssignUser_ShouldRemoveUser_WhenUserIdIsNull()
    {
        // Arrange
        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        task.AssignUser(null);

        // Assert
        Assert.Null(task.AssignedUserId);
    }

    [Fact]
    public void AssignUser_ShouldThrowInvalidTaskAssignmentException_WhenTaskIsDone()
    {
        // Arrange
        var assignedUserId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            assignedUserId
        );

        task.Start();
        task.Complete();

        // Act & Assert
        Assert.Throws<InvalidTaskAssignmentException>(() => task.AssignUser(assignedUserId));

        // Assert
        Assert.Equal(assignedUserId, task.AssignedUserId);
    }

    [Fact]
    public void AssignUser_ShouldThrowInvalidTaskAssignmentException_WhenTaskIsCancelled()
    {
        // Arrange
        var assignedUserId = Guid.NewGuid();

        var task = new TodoTask(
            "Test task",
            "Test description",
            Guid.NewGuid(),
            assignedUserId
        );

        task.Cancel();

        // Act & Assert
        Assert.Throws<InvalidTaskAssignmentException>(() => task.AssignUser(assignedUserId));

        // Assert
        Assert.Equal(assignedUserId, task.AssignedUserId);
    }
}

