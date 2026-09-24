using Rivulus.Domain.Entities;
namespace Rivulus.Application;
public interface ITaskCommentRepository
{
    Task AddAsync(TaskComment comment, CancellationToken cancellationToken = default);
    Task<List<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskComment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskComment comment, CancellationToken cancellationToken = default);
}
