using Rivulus.Domain.Entities;
namespace Rivulus.Application;
public interface IUserNotificationRepository
{
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default);
    Task<List<UserNotification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserNotification notification, CancellationToken cancellationToken = default);
}
