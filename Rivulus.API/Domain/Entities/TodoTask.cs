using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Rivulus.Domain.Exceptions;

namespace Rivulus.Domain.Entities
{
    public class TodoTask
    {
        public long Version { get; private set; }
        internal void AdvanceVersion() => Version++;

        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public TodoStatus Status { get; private set; }
        [BsonDefaultValue(TaskPriority.Medium)]
        public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; private set; }

        [BsonRepresentation(BsonType.String)]
        public Guid ProjectId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? AssignedUserId {get; private set; }

        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public TodoTask(string title, string description, Guid projectId, Guid? assignedUserId,
            TaskPriority priority = TaskPriority.Medium, DateTime? dueDate = null)
        {
            Title = title;
            Description = description;
            Status = TodoStatus.Todo;
            ProjectId = projectId;
            AssignedUserId = assignedUserId;
            Priority = priority;
            DueDate = NormalizeDueDate(dueDate);

            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }

        public void Update(string? title, string? description) => Update(title, description, Priority, DueDate);

        public void Update(string? title, string? description, TaskPriority priority, DateTime? dueDate)
        {
            var oldTitle = Title;
            var oldDescription = Description;
            var oldPriority = Priority;
            var oldDueDate = DueDate;

            if(Status == TodoStatus.Cancelled || Status == TodoStatus.Done)
            {
                throw new InvalidTaskUpdateException(
                    $"Cannot update a {Status} task."
                );
            }
            if(title != null)
                Title = title;
            if(description != null)
                Description = description;
            Priority = priority;
            DueDate = NormalizeDueDate(dueDate);

            if(oldTitle != Title || oldDescription != Description || oldPriority != Priority || oldDueDate != DueDate)
                UpdatedAt = DateTime.UtcNow;
        }

        private static DateTime? NormalizeDueDate(DateTime? value) => value is null
            ? null
            : DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);

        public void Delete()
        {
            if (DeletedAt != null)
            {
                throw new InvalidTaskDeletionException(
                    "Cannot delete a task that has already been deleted."
                );
            }

            var now = DateTime.UtcNow;
            DeletedAt = now;
            UpdatedAt = now;
        }

        public void AssignUser(Guid? userId)
        {
            var oldAssignedUserId = AssignedUserId;
            if(Status == TodoStatus.Cancelled || Status == TodoStatus.Done)
            {
                throw new InvalidTaskAssignmentException(
                    "Cannot change the assigned user of a completed or canceled task."
                );
            }

            AssignedUserId = userId;

            if(oldAssignedUserId!=AssignedUserId)
                UpdatedAt = DateTime.UtcNow;
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
            UpdatedAt = DateTime.UtcNow;
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
            UpdatedAt = DateTime.UtcNow;
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
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
