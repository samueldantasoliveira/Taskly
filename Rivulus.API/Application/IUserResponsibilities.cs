namespace Rivulus.Application;

public interface IUserResponsibilities
{
    Task<bool> HasPendingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ReleaseActiveTasksAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
}
