using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;
        public UserRepository(MongoDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(User user)
        {
            
            throw new NotImplementedException();
        }
    }
}
