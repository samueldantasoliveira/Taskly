using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Taskly.Domain.Entities
{
    public class TodoTask
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string? Description { get; set; }
        public TodoStatus Status { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid ProjectId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? AssignedUserId {get; set; }

        public TodoTask(string title, string description, Guid projectId, Guid assignedUserId)
        {
            Title = title;
            Description = description;
            Status = TodoStatus.Pending;
            ProjectId = projectId;
            AssignedUserId = assignedUserId;
        }
    }
}