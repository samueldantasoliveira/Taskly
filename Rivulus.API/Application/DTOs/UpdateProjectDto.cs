using Rivulus.Domain;

namespace Rivulus.Application.DTOs
{
public class UpdateProjectDto
    {
        public long? Version { get; set; }
        public Guid? OwnerId { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }
        public ProjectStatus? Status { get; set; }

        public Guid? TeamId { get; set; }

    }
}
