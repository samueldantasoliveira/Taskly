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

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _context.Projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(Project project)
         {
            var update = Builders<Project>.Update
                .Set(p => p.Name, project.Name)
                .Set(p => p.Description, project.Description)
                .Set(p => p.OwnerId, project.OwnerId)
                .Set(p => p.Status, project.Status)
                .Set(p => p.TeamId, project.TeamId)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Projects.UpdateOneAsync(
                p => p.Id == project.Id
                && project.DeletedAt == null
                && p.UpdatedAt == project.UpdatedAt,
                update
            );
            return result.MatchedCount == 1;
        }
    }
}
