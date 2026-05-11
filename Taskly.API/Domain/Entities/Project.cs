using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Taskly.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        
        public Guid OwnerId { get; private set; } 

        public ProjectStatus Status { get; private set; } 

        public Guid TeamId { get; private set; }

        public DateTime CreatedAt { get; private set;}
        public DateTime UpdatedAt { get; private set;}
        public DateTime? DeletedAt { get; private set; }

        protected Project(){ }
        public Project(string name, string description, Guid teamId, ProjectStatus status, Guid ownerId)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TeamId = teamId;
            Status = status;
            OwnerId = ownerId;

            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }

        public void Update(string? name, string? description, Guid? ownerId, ProjectStatus? status, Guid? teamId)
        {
            if (name != null)
                Name = name;
            if (description != null)
                Description = description;
            if (ownerId != null)
                OwnerId = ownerId.Value;
            if (status != null)
                Status = status.Value;
            if (teamId != null)
                TeamId = teamId.Value;

            UpdatedAt = DateTime.UtcNow;
        }

    }
}
