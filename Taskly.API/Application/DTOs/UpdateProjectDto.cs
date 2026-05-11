using Taskly.Domain;

namespace Taskly.Application.DTOs
{
    public class UpdateProjectDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }
        public Guid? OwnerId { get; set; }
        public ProjectStatus? Status { get; set; }

        public Guid? TeamId { get; set; }

    }
}