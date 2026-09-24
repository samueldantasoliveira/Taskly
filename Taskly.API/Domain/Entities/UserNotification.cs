using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Taskly.Domain.Entities;
public class UserNotification
{
    [BsonRepresentation(BsonType.String)] public Guid Id { get; private set; }
    [BsonRepresentation(BsonType.String)] public Guid UserId { get; private set; }
    public string Message { get; private set; } = null!;
    public string Link { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    protected UserNotification() { }
    public UserNotification(Guid userId, string message, string link) { Id = Guid.NewGuid(); UserId = userId; Message = message; Link = link; CreatedAt = DateTime.UtcNow; }
    public void MarkAsRead() => ReadAt ??= DateTime.UtcNow;
}
