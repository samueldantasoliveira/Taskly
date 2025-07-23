namespace Taskly.Application.DTOs;

public class AddMemberResponseDto
{
    public Guid TeamId { get; init; }
    public Guid UserId { get; init; }
    public DateTime AddedAt { get; init; }
}