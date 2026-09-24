using Taskly.Domain;

namespace Taskly.Application.DTOs;

public class MyWorkDashboardResponseDto
{
    public int TodoCount { get; init; }
    public int InProgressCount { get; init; }
    public int OverdueCount { get; init; }
    public List<MyWorkItemResponseDto> Items { get; init; } = [];
}

public class MyWorkItemResponseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public TodoStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = null!;
    public string TeamName { get; init; } = null!;
}
