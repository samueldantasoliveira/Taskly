using Taskly.Application.DTOs;
namespace Taskly.Application;
public class UserNotificationService(IUserNotificationRepository repository)
{
    public async Task<List<UserNotificationResponseDto>> GetAsync(Guid userId, CancellationToken ct = default) => (await repository.GetByUserIdAsync(userId, ct)).Select(Map).ToList();
    public async Task<bool> MarkReadAsync(Guid id, Guid userId, CancellationToken ct = default) { var value = await repository.GetByIdAsync(id, ct); if (value == null || value.UserId != userId) return false; value.MarkAsRead(); await repository.UpdateAsync(value, ct); return true; }
    private static UserNotificationResponseDto Map(Domain.Entities.UserNotification value) => new() { Id = value.Id, Message = value.Message, Link = value.Link, CreatedAt = value.CreatedAt, ReadAt = value.ReadAt };
}
