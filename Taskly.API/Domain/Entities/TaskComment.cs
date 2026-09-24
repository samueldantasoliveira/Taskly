using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Taskly.Domain.Entities;

public class TaskComment
{
    [BsonRepresentation(BsonType.String)] public Guid Id { get; private set; }
    [BsonRepresentation(BsonType.String)] public Guid TaskId { get; private set; }
    [BsonRepresentation(BsonType.String)] public Guid AuthorId { get; private set; }
    public string AuthorName { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    protected TaskComment() { }
    public TaskComment(Guid taskId, Guid authorId, string authorName, string content)
    {
        Id = Guid.NewGuid(); TaskId = taskId; AuthorId = authorId; AuthorName = authorName;
        Content = content.Trim(); CreatedAt = UpdatedAt = DateTime.UtcNow;
    }
    public void Update(string content) { Content = content.Trim(); UpdatedAt = DateTime.UtcNow; }
    public void Delete() { DeletedAt = DateTime.UtcNow; UpdatedAt = DeletedAt.Value; }
}
