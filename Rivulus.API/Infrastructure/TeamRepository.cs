using MongoDB.Driver;
using Rivulus.Application;
using Rivulus.Domain.Entities;
using System.Linq.Expressions;

namespace Rivulus.Infrastructure
{
    public class TeamRepository : ITeamRepository
    {
        private readonly MongoDbContext _context;
        public TeamRepository(MongoDbContext context) 
        {
            _context = context;
        }
        public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
        {
            await _context.Teams.InsertOneAsync(team, cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var update = Builders<Team>.Update
                .Set(t => t.DeletedAt, DateTime.UtcNow);

            var result = await _context.Teams.UpdateOneAsync(
                t => t.Id == id && t.DeletedAt == null,
                update,
                cancellationToken: cancellationToken
            );
            return result.ModifiedCount == 1;
        }

        public async Task<List<Team>> GetUserTeamsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Team>.Filter.And(
                Builders<Team>.Filter.Eq(team => team.DeletedAt, null),
                Builders<Team>.Filter.Eq<string>("UserIds", userId.ToString())
            );

            return await _context.Teams
                .Find(filter)
                .ToListAsync(cancellationToken);
        }

        public async Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Teams.Find(BaseFilter(t => t.Id == id)).FirstOrDefaultAsync(cancellationToken);
            
        }

        public async Task<bool> UpdateAsync(Team updatedTeam, CancellationToken cancellationToken = default)
        {
            var filter = BaseFilter(t => t.Id == updatedTeam.Id);
            var versionedFilter = ConcurrencyGuard.WithVersion(filter, updatedTeam.Version);
            updatedTeam.AdvanceVersion();
            var result = await _context.Teams.ReplaceOneAsync(
                versionedFilter,
                updatedTeam,
                cancellationToken: cancellationToken);
            return await ConcurrencyGuard.CheckWrite(_context.Teams, filter, result.MatchedCount, cancellationToken);
        }
            

        private FilterDefinition<Team> BaseFilter(Expression<Func<Team, bool>> filter)
{
            return Builders<Team>.Filter.And(
                filter,
                Builders<Team>.Filter.Eq(t => t.DeletedAt, null)
            );
        }
    }
}
