using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Taskly.Domain.Entities
{
    public class Team
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public Guid OwnerId {get; private set; }
        [BsonRepresentation(BsonType.String)]
        public List<Guid> UserIds { get; set; } = new();

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public Team(string name, Guid ownerId)
        {
            Id = Guid.NewGuid();
            OwnerId = ownerId;
            UserIds.Add(ownerId);
            Name = name;
            IsActive = true;
            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }
        public void Update(string? name, bool? isActive)
        {
            if (name != null)
                Name = name;

            if (isActive != null)
                IsActive = isActive.Value;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
