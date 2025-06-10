using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Taskly.Domain.Entities
{
    public class Project
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid TeamId { get; set; }

        public Project(string name, string description, Guid teamId, ProjectStatus status)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TeamId = teamId;
            Status = status;
        }
    }
}
