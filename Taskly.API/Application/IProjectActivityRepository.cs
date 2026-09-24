using Taskly.Domain.Entities;

namespace Taskly.Application;

public interface IProjectActivityRepository
{
    Task AddAsync(ProjectActivity activity, CancellationToken cancellationToken = default);
    Task<List<ProjectActivity>> GetByProjectIdAsync(Guid projectId, int limit, CancellationToken cancellationToken = default);
}
