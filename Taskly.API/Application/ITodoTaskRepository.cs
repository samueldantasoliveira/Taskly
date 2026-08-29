using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface ITodoTaskRepository
    {
        Task<List<TodoTask>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken);
        Task<TodoTask?> GetByIdAsync(Guid id);
        Task AddAsync(TodoTask task);
        Task<bool> UpdateAsync(TodoTask task);
    }
}
