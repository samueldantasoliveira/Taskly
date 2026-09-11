using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
using System.Linq.Expressions;
using System.ComponentModel;

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

        public async Task<PagedResult<TodoTask>> GetByProjectIdAsync(
            Guid projectId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var filter = BaseFilter(t => t.ProjectId == projectId); 
            var sort = Builders<TodoTask>.Sort
                .Descending(t => t.CreatedAt)
                .Ascending(t => t.Id);

            var skip = checked((page - 1) * pageSize);
            List<TodoTask> tasks = await _context.TodoTasks
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            var totalCount = await _context.TodoTasks
                .CountDocumentsAsync(
                    filter,
                    cancellationToken: cancellationToken);

            return new PagedResult<TodoTask>
            {
                Items = tasks, TotalCount = totalCount
            };
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
