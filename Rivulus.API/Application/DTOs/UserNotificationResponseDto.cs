namespace Rivulus.Application.DTOs;
public class UserNotificationResponseDto { public Guid Id { get; init; } public string Message { get; init; } = null!; public string Link { get; init; } = null!; public DateTime CreatedAt { get; init; } public DateTime? ReadAt { get; init; } }
