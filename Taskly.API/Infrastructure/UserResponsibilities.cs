using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure;

public class UserResponsibilities(MongoDbContext context) : IUserResponsibilities
{
    public async Task<bool> HasPendingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (await context.Teams.Find(t => t.DeletedAt == null && t.OwnerId == userId).AnyAsync(cancellationToken))
            return true;

        var assignedProjectIds = await context.TodoTasks
            .Find(t => t.DeletedAt == null && t.AssignedUserId == userId &&
                (t.Status == TodoStatus.Todo || t.Status == TodoStatus.InProgress))
            .Project(t => t.ProjectId).ToListAsync(cancellationToken);
        var teamIds = await context.Projects
            .Find(p => p.DeletedAt == null && (p.OwnerId == userId || assignedProjectIds.Contains(p.Id)))
            .Project(p => p.TeamId).ToListAsync(cancellationToken);
        return await context.Teams.Find(t => t.DeletedAt == null && teamIds.Contains(t.Id)).AnyAsync(cancellationToken);
    }

    public async Task ReleaseActiveTasksAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var projects = await context.Projects.Find(p => p.TeamId == teamId && p.DeletedAt == null)
            .Project(p => p.Id).ToListAsync(cancellationToken);
        await context.TodoTasks.UpdateManyAsync(
            t => t.DeletedAt == null && projects.Contains(t.ProjectId) && t.AssignedUserId == userId &&
                (t.Status == TodoStatus.Todo || t.Status == TodoStatus.InProgress),
            Builders<TodoTask>.Update.Set(t => t.AssignedUserId, null)
                .Set(t => t.UpdatedAt, DateTime.UtcNow).Inc(t => t.Version, 1),
            cancellationToken: cancellationToken);
    }
}
