using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly MongoDbContext _context;

        public ProjectRepository(MongoDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Project project)
        {
            await _context.Projects.InsertOneAsync(project);
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Project> GetByIdAsync(Guid id)
        {
            return await _context.Projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public Task UpdateAsync(Project project)
        {
            throw new NotImplementedException();
        }
    }
}
