using Rivulus.Domain.Entities;
namespace Rivulus.Application;
public interface ITeamInvitationRepository
{
    Task AddAsync(TeamInvitation invitation, CancellationToken cancellationToken = default);
    Task<TeamInvitation?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<TeamInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TeamInvitation>> GetPendingByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingAsync(Guid teamId, string email, CancellationToken cancellationToken = default);
    Task UpdateAsync(TeamInvitation invitation, CancellationToken cancellationToken = default);
}
