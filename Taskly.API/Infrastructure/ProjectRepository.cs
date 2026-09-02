using System.Linq.Expressions;
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
        public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            await _context.Projects.InsertOneAsync(project, cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var update = Builders<Project>.Update
                .Set(p => p.DeletedAt, DateTime.UtcNow);

            var result = await _context.Projects.UpdateOneAsync(
                p => p.Id == id && p.DeletedAt == null,
                update,
                cancellationToken: cancellationToken
            );

            return result.ModifiedCount == 1;

        }

        public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Projects.Find(BaseFilter(p => p.Id == id)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Project>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .Find(BaseFilter(p => p.TeamId == teamId))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(Project project, CancellationToken cancellationToken = default)
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
                && p.DeletedAt == null,
                update,
                cancellationToken: cancellationToken
            );
            return result.MatchedCount == 1;
        }

        private FilterDefinition<Project> BaseFilter(Expression<Func<Project, bool>> filter)
{
            return Builders<Project>.Filter.And(
                filter,
                Builders<Project>.Filter.Eq(p => p.DeletedAt, null)
            );
        }
    }
}
