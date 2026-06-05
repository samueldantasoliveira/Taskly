using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetAllAsync();
        Task<Team?> GetByIdAsync(Guid id);
        Task AddAsync(Team team);
        Task<bool> UpdateAsync(Team team);
        Task<bool> AddMemberAsync(Guid teamId, Guid userId);
        Task<bool> RemoveMemberAsync(Guid teamId, Guid userId);
        Task<bool> DeleteAsync(Guid id);
    }
}
