using System.ComponentModel.DataAnnotations;

namespace Rivulus.Application.DTOs;

public class CreateProjectDto
{
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Description { get; set; }
    public Guid TeamId { get; set; }
}