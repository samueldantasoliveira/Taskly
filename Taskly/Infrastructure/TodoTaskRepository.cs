using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;

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

        public async Task<List<TodoTask>> GetAllByProjectAsync(Guid projectId)
        {
            return await _context.TodoTasks.Find(t => t.ProjectId == projectId).ToListAsync();
        }

        public async Task<TodoTask> GetByIdAsync(Guid id)
        {
            return await _context.TodoTasks.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(TodoTask task)
        {
            var result = await _context.TodoTasks.ReplaceOneAsync(t => t.Id == task.Id, task);
            return result.ModifiedCount > 0;
        }
    }
}
