namespace Rivulus.Application.DTOs;
public class TaskCommentResponseDto { public Guid Id { get; init; } public Guid AuthorId { get; init; } public string AuthorName { get; init; } = null!; public string Content { get; init; } = null!; public DateTime CreatedAt { get; init; } public DateTime UpdatedAt { get; init; } }
