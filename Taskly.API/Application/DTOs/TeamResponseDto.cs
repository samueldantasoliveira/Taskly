using Taskly.Domain;

namespace Taskly.Application.DTOs;

public class TeamResponseDto{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public bool IsActive { get; init; }
    public Guid OwnerId {get; init; }
    public List<Guid> UserIds { get; init; } = new();
}
