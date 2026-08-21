using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Taskly.Domain.Exceptions;

namespace Taskly.Domain.Entities
{
    public class TodoTask
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TodoStatus Status { get; private set; }

        [BsonRepresentation(BsonType.String)]
        public Guid ProjectId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? AssignedUserId {get; private set; }

        public TodoTask(string title, string description, Guid projectId, Guid? assignedUserId)
        {
            Title = title;
            Description = description;
            Status = TodoStatus.Todo;
            ProjectId = projectId;
            AssignedUserId = assignedUserId;
        }

        public void AssignUser(Guid? userId)
        {
            if(Status == TodoStatus.Cancelled || Status == TodoStatus.Done)
            {
                throw new InvalidTaskAssignmentException(
                    "Cannot change the assigned user of a completed or canceled task."
                );
            }

            AssignedUserId = userId;
        }

        public void Start()
        {
            if(Status != TodoStatus.Todo)
            {
                throw new InvalidTaskTransitionException(
                    $"Cannot transition task from '{Status}' to 'InProgress'."
                );
            }
            
            Status = TodoStatus.InProgress;
            
        }

        public void Complete()
        {
            if(Status != TodoStatus.InProgress)
            {
                throw new InvalidTaskTransitionException(
                    $"Cannot transition task from '{Status}' to 'Done'."
                );
            }

            if(AssignedUserId == null)
            {
                throw new InvalidTaskCompletionException(
                    "Cannot complete a task without an assigned user."
                );
            }

            Status = TodoStatus.Done;
        }

        public void Cancel()
        {
            if(Status != TodoStatus.Todo && Status != TodoStatus.InProgress)
            {
                throw new InvalidTaskTransitionException(
                    $"Cannot transition task from '{Status}' to 'Cancelled'."
                );
            }

            Status = TodoStatus.Cancelled;
        }
    }
}