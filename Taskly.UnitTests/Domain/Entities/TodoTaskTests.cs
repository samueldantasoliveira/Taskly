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
        var task = new TodoTask("Title Test", "Description Test", Guid.NewGuid(), Guid.NewGuid());
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
            Guid.NewGuid()
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
            Guid.NewGuid()
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
    public void Complete_ShouldThrowInvalidTaskCompletionException_WhenTaskHasNoAssignedUser()
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
        Assert.Throws<InvalidTaskCompletionException>(() => task.Complete());

        // Assert
        Assert.Equal(TodoStatus.InProgress, task.Status);
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
            Guid.NewGuid()
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

    [Fact]
    public void Update_ShouldChangeTitleAndDescription_WhenTaskIsTodo()
    {
        // Arrange
        var task = new TodoTask(
            "Old title",
            "Old description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        task.Update("New title", "New description");

        // Assert
        Assert.Equal("New title", task.Title);
        Assert.Equal("New description", task.Description);
    }

    [Fact]
    public void Update_ShouldThrowInvalidTaskUpdateException_WhenTaskIsDone()
    {
        // Arrange
        var task = new TodoTask(
            "Old title",
            "Old description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        task.Start();
        task.Complete();

        // Act & Assert
        Assert.Throws<InvalidTaskUpdateException>(() => task.Update("New title", "New description"));
        Assert.Equal("Old title", task.Title);
        Assert.Equal("Old description", task.Description);
    }

    [Fact]
    public void Update_ShouldThrowInvalidTaskUpdateException_WhenTaskIsCancelled()
    {
        // Arrange
        var task = new TodoTask(
            "Old title",
            "Old description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        task.Start();
        task.Complete();

        // Act & Assert
        Assert.Throws<InvalidTaskUpdateException>(() => task.Update("New title", "New description"));
        Assert.Equal("Old title", task.Title);
        Assert.Equal("Old description", task.Description);
    }

    [Fact]
    public void TodoTask_ShouldSetCreatedAtAndUpdatedAt_WhenCreated()
    {
        // Act
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Assert
        Assert.NotEqual(default, task.CreatedAt);
        Assert.NotEqual(default, task.UpdatedAt);
        Assert.Equal(task.CreatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldUpdateUpdatedAt_WhenTitleChanges()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.Update("New Title", null);

        // Assert
        Assert.NotEqual(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldUpdateUpdatedAt_WhenDescriptionChanges()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.Update(null, "New Description");

        // Assert
        Assert.NotEqual(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldNotUpdateUpdatedAt_WhenNoDataChanges()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.Update(null, null);

        // Assert
        Assert.Equal(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void AssignUser_ShouldUpdateUpdatedAt_WhenAssignedUserChanges()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            null
        );
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.AssignUser(Guid.NewGuid());

        // Assert
        Assert.NotEqual(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void AssignUser_ShouldNotUpdateUpdatedAt_WhenAssignedUserDoesNotChange()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            null
        );
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.AssignUser(null);

        // Assert
        Assert.Equal(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Start_ShouldUpdateUpdatedAt_WhenTaskIsTodo()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.Start();

        // Assert
        Assert.NotEqual(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Complete_ShouldUpdateUpdatedAt_WhenTaskIsInProgress()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        task.Start();
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.Complete();

        // Assert
        Assert.NotEqual(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Cancel_ShouldUpdateUpdatedAt_WhenTaskIsTodo()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.Cancel();

        // Assert
        Assert.NotEqual(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Cancel_ShouldUpdateUpdatedAt_WhenTaskIsInProgress()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        task.Start();
        var oldUpdatedAt = task.UpdatedAt;

        // Act
        task.Cancel();

        // Assert
        Assert.NotEqual(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Start_ShouldNotUpdateUpdatedAt_WhenTaskIsNotTodo()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );
        task.Start();
        var oldUpdatedAt = task.UpdatedAt;

        // Act & Assert
        Assert.Throws<InvalidTaskTransitionException>(() => task.Start());

        Assert.Equal(oldUpdatedAt, task.UpdatedAt);
    }

    [Fact]
    public void Delete_ShouldSetDeletedAtAndUpdateUpdatedAt()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        // Act
        task.Delete();

        // Assert
        Assert.Equal(task.UpdatedAt, task.DeletedAt);
        Assert.NotNull(task.DeletedAt);
    }

    [Fact]
    public void Delete_ShouldThrowInvalidTaskDeletionException_WhenTaskIsAlreadyDeleted()
    {
        // Arrange
        var task = new TodoTask(
            "Title",
            "Description",
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        task.Delete();

        // Act & Assert
        Assert.Throws<InvalidTaskDeletionException>(() => task.Delete());
    }

    
}

