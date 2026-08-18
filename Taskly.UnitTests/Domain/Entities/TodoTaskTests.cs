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
}