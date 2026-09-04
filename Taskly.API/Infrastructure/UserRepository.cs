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
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var update = Builders<User>.Update
                .Set(u => u.DeletedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(
                u => u.Id == id && u.DeletedAt == null,
                update,
                cancellationToken: cancellationToken
            );

            return result.ModifiedCount == 1;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users.Find(BaseFilter(u => u.Id == id)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<User>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            var filter = Builders<User>.Filter.In(user => user.Id, ids);
            return await _context.Users
                .Find(BaseFilter(filter))
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users.Find(BaseFilter(u => u.Email == email)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _context.Users.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var update = Builders<User>.Update
                .Set(u => u.Name, user.Name)
                .Set(u => u.Email, user.Email)
                .Set(u => u.PasswordHash, user.PasswordHash)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(
                u => u.Id == user.Id 
                && u.DeletedAt == null,
                //&& u.UpdatedAt == user.UpdatedAt,
                update,
                cancellationToken: cancellationToken
            );
            
            return result.MatchedCount == 1;
        }       
        private FilterDefinition<User> BaseFilter(Expression<Func<User, bool>> filter)
        {
            return Builders<User>.Filter.And(
                filter,
                Builders<User>.Filter.Eq(u => u.DeletedAt, null)
            );
        }

        private FilterDefinition<User> BaseFilter(FilterDefinition<User> filter)
        {
            return Builders<User>.Filter.And(
                filter,
                Builders<User>.Filter.Eq(user => user.DeletedAt, null)
            );
        }
    }
}
