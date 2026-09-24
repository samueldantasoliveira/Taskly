namespace Rivulus.Application.DTOs;
public class CreateTeamInvitationDto { public string Email { get; set; } = null!; }
public class TeamInvitationResponseDto { public Guid Id { get; init; } public Guid TeamId { get; init; } public string TeamName { get; init; } = null!; public string Email { get; init; } = null!; public string? Token { get; init; } public DateTime ExpiresAt { get; init; } public DateTime CreatedAt { get; init; } }
