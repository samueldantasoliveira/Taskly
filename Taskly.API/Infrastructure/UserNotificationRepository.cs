using MongoDB.Driver;
using Taskly.Application;
using Taskly.Domain.Entities;
namespace Taskly.Infrastructure;
public class UserNotificationRepository(MongoDbContext context) : IUserNotificationRepository
{
    public Task AddAsync(UserNotification value, CancellationToken ct = default) => context.UserNotifications.InsertOneAsync(value, cancellationToken: ct);
    public Task<List<UserNotification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) => context.UserNotifications.Find(x => x.UserId == userId).SortByDescending(x => x.CreatedAt).Limit(50).ToListAsync(ct);
    public Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken ct = default) => context.UserNotifications.Find(x => x.Id == id).FirstOrDefaultAsync(ct)!;
    public Task UpdateAsync(UserNotification value, CancellationToken ct = default) => context.UserNotifications.ReplaceOneAsync(x => x.Id == value.Id, value, cancellationToken: ct);
}
