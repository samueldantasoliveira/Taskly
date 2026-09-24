using Rivulus.Domain.Entities;
using Rivulus.Application.Queries;

namespace Rivulus.Application
{
    public interface ITodoTaskRepository
    {
        Task<PagedResult<TodoTask>> GetByProjectIdAsync(
            Guid projectId,
            TodoTaskQuery query,
            CancellationToken cancellationToken = default);
        Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetAssignedUserIdsByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<TodoTask>> GetActiveAssignedToUserAsync(IEnumerable<Guid> projectIds, Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(TodoTask task, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TodoTask task, CancellationToken cancellationToken = default);
    }
}
