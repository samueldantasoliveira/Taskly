using Taskly.Infrastructure;
using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public class TeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;

        public TeamService(ITeamRepository teamRepository, IUserRepository userRepository)
        {
            _teamRepository = teamRepository;
            _userRepository = userRepository;
        }

        public async Task AddTeamAsync(Team team)
        {
            if (String.IsNullOrEmpty(team.Name))
                throw new ArgumentException("Name must not be empty");

            await _teamRepository.AddAsync(team);
        }

        public async Task<bool> AddMemberAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null || !team.IsActive)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                return false;

            if (team.UserIds.Contains(userId))
                return false;

            team.UserIds.Add(userId);
            await _teamRepository.UpdateAsync(team);

            return true;
        }
        //outras funções 
    }
}
