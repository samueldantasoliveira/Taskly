using System.ComponentModel.DataAnnotations;
using Taskly.Domain;

namespace Taskly.Application.DTOs
{
public class UpdateTodoTaskDto
    {
        public long? Version { get; set; }
        [Required]
        [StringLength(100)]
        public required string Title { get; set; }
        [StringLength(500)]
        public required string Description { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; set; }
    }
}
