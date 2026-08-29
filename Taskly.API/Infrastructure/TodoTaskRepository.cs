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
        public async Task AddAsync(TodoTask todoTask)
        {
            await _context.TodoTasks.InsertOneAsync(todoTask);
        }

        public async Task<TodoTask?> GetByIdAsync(Guid id)
        {
            return await _context.TodoTasks.Find(BaseFilter(t => t.Id == id)).FirstOrDefaultAsync();
        }

        public async Task<List<TodoTask>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            return await _context.TodoTasks
                .Find(BaseFilter(t => t.ProjectId == projectId))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(TodoTask task)
        {
            var result = await _context.TodoTasks.ReplaceOneAsync(
                BaseFilter(t => t.Id == task.Id), 
                task);
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
