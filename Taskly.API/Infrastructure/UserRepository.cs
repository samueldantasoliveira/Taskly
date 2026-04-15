using System.Linq.Expressions;
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

        public async Task<bool> DeleteAsync(Guid id)
        {
            var update = Builders<User>.Update
                .Set(u => u.DeletedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(
                u => u.Id == id && u.DeletedAt == null,
                update
            );

            return result.ModifiedCount == 1;
        }

        public Task<List<User>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.Find(BaseFilter(u => u.Id == id)).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.Find(BaseFilter(u => u.Email == email)).FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _context.Users.Find(filter).AnyAsync();
        }

        public async Task UpdateAsync(User user)
        {
            
            throw new NotImplementedException();
        }
        private FilterDefinition<User> BaseFilter(Expression<Func<User, bool>> filter)
{
            return Builders<User>.Filter.And(
                filter,
                Builders<User>.Filter.Eq(u => u.DeletedAt, null)
            );
        }
    }
}
