using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
namespace Taskly.Infrastructure;
public class TaskCommentRepository(MongoDbContext context) : ITaskCommentRepository
{
    public Task AddAsync(TaskComment comment, CancellationToken cancellationToken = default) => context.TaskComments.InsertOneAsync(comment, cancellationToken: cancellationToken);
    public Task<List<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default) => context.TaskComments.Find(comment => comment.TaskId == taskId).SortBy(comment => comment.CreatedAt).ToListAsync(cancellationToken);
}
