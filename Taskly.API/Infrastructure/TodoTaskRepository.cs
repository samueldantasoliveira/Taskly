using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
using System.Linq.Expressions;

namespace Taskly.Infrastructure
{
    public class TodoTaskRepository : ITodoTaskRepository
    {
        private readonly MongoDbContext _context;

        public TodoTaskRepository(MongoDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(TodoTask todoTask, CancellationToken cancellationToken = default)
        {
            await _context.TodoTasks.InsertOneAsync(todoTask, cancellationToken: cancellationToken);
        }

        public async Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.TodoTasks.Find(BaseFilter(t => t.Id == id)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TodoTask>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            return await _context.TodoTasks
                .Find(BaseFilter(t => t.ProjectId == projectId))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(TodoTask task, CancellationToken cancellationToken = default)
        {
            var result = await _context.TodoTasks.ReplaceOneAsync(
                BaseFilter(t => t.Id == task.Id), 
                task,
                cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }

        private FilterDefinition<TodoTask> BaseFilter(Expression<Func<TodoTask, bool>> filter)
{
            return Builders<TodoTask>.Filter.And(
                filter,
                Builders<TodoTask>.Filter.Eq(t => t.DeletedAt, null)
            );
        }
    }
}
