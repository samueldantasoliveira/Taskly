using Rivulus.Domain;

namespace Rivulus.Application.Queries;
public class TodoTaskQuery
{
      public int Page { get; init; } = 1;
      public int PageSize { get; init; } = 20;

      public string? Title { get; init; }
      public TodoStatus? Status { get; init; }
      public Guid? AssigneeId { get; init; }

      public TodoTaskSortBy SortBy { get; init; } = TodoTaskSortBy.CreatedAt;
      public TodoTaskSortDirection SortDirection { get; init; } =
      TodoTaskSortDirection.Descending;
}
