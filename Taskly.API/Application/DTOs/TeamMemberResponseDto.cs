namespace Taskly.Application.DTOs;

public class TeamMemberResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public bool IsOwner { get; init; }
}
