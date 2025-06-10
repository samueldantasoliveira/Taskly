using System.ComponentModel.DataAnnotations;

namespace Taskly.Application.DTOs
{
    public class CreateTodoTaskDto
    {
        [Required]
        [StringLength(100)]
        public required string Title { get; set; }
        [StringLength(500)]
        public required string Description { get; set; }
        [Required]
        public Guid ProjectId { get; set; }

        public Guid? AssignedUserId { get; set; }
        
    }
}
