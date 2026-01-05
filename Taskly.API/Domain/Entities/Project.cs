using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Taskly.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public Guid OwnerId { get; set; }

        public ProjectStatus Status { get; set; }

        public Guid TeamId { get; set; }

        public Project(string name, string description, Guid teamId, ProjectStatus status, Guid ownerId)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TeamId = teamId;
            Status = status;
            OwnerId = ownerId;
        }
    }
}
