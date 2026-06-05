using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure
{
    public class TeamRepository : ITeamRepository
    {
        private readonly MongoDbContext _context;
        public TeamRepository(MongoDbContext context) 
        {
            _context = context;
        }
        public async Task AddAsync(Team team)
        {
            await _context.Teams.InsertOneAsync(team);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var update = Builders<Team>.Update
                .Set(t => t.DeletedAt, DateTime.UtcNow);

            var result = await _context.Teams.UpdateOneAsync(
                t => t.Id == id && t.DeletedAt == null,
                update
            );
            return result.ModifiedCount == 1;
        }

        public Task<List<Team>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Team?> GetByIdAsync(Guid id)
        {
            return await _context.Teams.Find(t => t.Id == id).FirstOrDefaultAsync();
            
        }

        public async Task<bool> UpdateAsync(Team updatedTeam)
        {
            var update = Builders<Team>.Update
                .Set(t => t.Name, updatedTeam.Name)
                .Set(t => t.IsActive, updatedTeam.IsActive)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Teams.UpdateOneAsync(
                t => t.Id == updatedTeam.Id
                && t.DeletedAt == null,
                update
            );
            return result.MatchedCount == 1;
        }

        public async Task<bool> AddMemberAsync(Guid teamId, Guid userId)
        {
            var update = Builders<Team>.Update
                .Push(t => t.UserIds, userId)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Teams.UpdateOneAsync(
                t => t.Id == teamId && t.DeletedAt == null,
                update
            );

            return result.ModifiedCount == 1;
        }

        public async Task<bool> RemoveMemberAsync(Guid teamId, Guid userId)
        {
            var update = Builders<Team>.Update
                .Pull(t => t.UserIds, userId)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Teams.UpdateOneAsync(
                t => t.Id == teamId && t.DeletedAt == null,
                update
            );

            return result.ModifiedCount == 1;
        }
    }
}
