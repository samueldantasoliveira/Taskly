using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Project project, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Project project, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
