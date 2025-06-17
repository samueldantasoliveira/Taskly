using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Taskly.Domain.Entities
{
    public class Team
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        [BsonRepresentation(BsonType.String)]
        public List<Guid> UserIds { get; set; } = new();

        public Team(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            IsActive = true;
        }
    }
}
