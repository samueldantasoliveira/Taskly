using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetAllAsync();
        Task<Team> GetByIdAsync(Guid id);
        Task AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(Guid id);
    }
}
