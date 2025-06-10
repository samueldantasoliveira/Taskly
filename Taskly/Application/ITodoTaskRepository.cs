using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface ITodoTaskRepository
    {
        Task<List<TodoTask>> GetAllByProjectAsync(Guid projectId);
        Task<TodoTask> GetByIdAsync(Guid id);
        Task AddAsync(TodoTask task);
        Task<bool> UpdateAsync(TodoTask task);
    }
}
