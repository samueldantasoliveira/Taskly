namespace Rivulus.Application.DTOs;

public class ProjectActivityResponseDto
{
    public Guid Id { get; init; }
    public Guid ActorId { get; init; }
    public string ActorName { get; init; } = null!;
    public string? ActorAvatarKey { get; init; }
    public Guid? TaskId { get; init; }
    public string TaskTitle { get; init; } = null!;
    public string Description { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}
