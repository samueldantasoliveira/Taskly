using Rivulus.Domain.Entities;

namespace Rivulus.Application;

public interface IProjectActivityRepository
{
    Task AddAsync(ProjectActivity activity, CancellationToken cancellationToken = default);
    Task<List<ProjectActivity>> GetByProjectIdAsync(Guid projectId, int limit, CancellationToken cancellationToken = default);
}
