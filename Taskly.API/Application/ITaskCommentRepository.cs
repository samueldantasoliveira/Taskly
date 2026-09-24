using Taskly.Domain.Entities;
namespace Taskly.Application;
public interface ITaskCommentRepository
{
    Task AddAsync(TaskComment comment, CancellationToken cancellationToken = default);
    Task<List<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
}
