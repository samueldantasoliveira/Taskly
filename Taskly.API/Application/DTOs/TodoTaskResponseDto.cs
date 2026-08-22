using Taskly.Domain;

namespace Taskly.Application.DTOs;

public class TodoTaskResponseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public TodoStatus Status { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? AssignedUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}