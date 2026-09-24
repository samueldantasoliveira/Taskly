using Rivulus.Domain.Entities;

namespace Rivulus.Application
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetUserTeamsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Team team, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Team team, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
