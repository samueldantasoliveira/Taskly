using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
namespace Taskly.Infrastructure;
public class TaskCommentRepository(MongoDbContext context) : ITaskCommentRepository
{
    public Task AddAsync(TaskComment comment, CancellationToken cancellationToken = default) => context.TaskComments.InsertOneAsync(comment, cancellationToken: cancellationToken);
    public Task<List<TaskComment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default) => context.TaskComments.Find(comment => comment.TaskId == taskId && comment.DeletedAt == null).SortBy(comment => comment.CreatedAt).ToListAsync(cancellationToken);
    public Task<TaskComment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => context.TaskComments.Find(comment => comment.Id == id && comment.DeletedAt == null).FirstOrDefaultAsync(cancellationToken)!;
    public Task UpdateAsync(TaskComment comment, CancellationToken cancellationToken = default) => context.TaskComments.ReplaceOneAsync(current => current.Id == comment.Id && current.DeletedAt == null, comment, cancellationToken: cancellationToken);
}
