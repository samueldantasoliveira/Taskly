using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetUserTeamsAsync(Guid userId);
        Task<Team?> GetByIdAsync(Guid id);
        Task AddAsync(Team team);
        Task<bool> UpdateAsync(Team team);
        Task<bool> DeleteAsync(Guid id);
    }
}
