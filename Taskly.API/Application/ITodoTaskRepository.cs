using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface ITodoTaskRepository
    {
        Task<PagedResult<TodoTask>> GetByProjectIdAsync(
            Guid projectId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(TodoTask task, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TodoTask task, CancellationToken cancellationToken = default);
    }
}
