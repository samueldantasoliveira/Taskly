using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rivulus.Domain.Entities;

public class ProjectActivity
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; private set; }
    [BsonRepresentation(BsonType.String)]
    public Guid ProjectId { get; private set; }
    [BsonRepresentation(BsonType.String)]
    public Guid ActorId { get; private set; }
    public string ActorName { get; private set; } = null!;
    public string? ActorAvatarKey { get; private set; }
    [BsonRepresentation(BsonType.String)]
    public Guid? TaskId { get; private set; }
    public string TaskTitle { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    protected ProjectActivity() { }

    public ProjectActivity(Guid projectId, Guid actorId, string actorName, Guid? taskId, string taskTitle, string description, string? actorAvatarKey = null)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        ActorId = actorId;
        ActorName = actorName;
        ActorAvatarKey = actorAvatarKey;
        TaskId = taskId;
        TaskTitle = taskTitle;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
}
