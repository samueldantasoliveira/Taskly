using MongoDB.Driver;
using Rivulus.Application;
using Rivulus.Domain.Entities;

namespace Rivulus.Infrastructure;

public class ProjectActivityRepository(MongoDbContext context) : IProjectActivityRepository
{
    public Task AddAsync(ProjectActivity activity, CancellationToken cancellationToken = default) =>
        context.ProjectActivities.InsertOneAsync(activity, cancellationToken: cancellationToken);

    public Task<List<ProjectActivity>> GetByProjectIdAsync(Guid projectId, int limit, CancellationToken cancellationToken = default) =>
        context.ProjectActivities.Find(activity => activity.ProjectId == projectId)
            .SortByDescending(activity => activity.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
}
