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

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Team>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Team> GetByIdAsync(Guid id)
        {
            return await _context.Teams.Find(t => t.Id == id).FirstOrDefaultAsync();
            
        }

        public async Task UpdateAsync(Team updatedTeam)
        {
            await _context.Teams.ReplaceOneAsync(t => t.Id == updatedTeam.Id, updatedTeam);
        }
    }
}
