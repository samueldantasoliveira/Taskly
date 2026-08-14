using Taskly.Domain;

namespace Taskly.Application.DTOs;

public class ProjectResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public Guid OwnerId { get; init; }
    public ProjectStatus Status { get; init; }
    public Guid TeamId { get; init; }
}