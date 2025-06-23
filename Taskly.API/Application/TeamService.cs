using Taskly.Domain.Entities;
using Taskly.Application.Results;
using Taskly.Infrastructure;

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

        public async Task<OperationResult<AddTeamFailureReason>> AddTeamAsync(Team team)
        {
            if (String.IsNullOrWhiteSpace(team.Name))
                return OperationResult<AddTeamFailureReason>.Fail(AddTeamFailureReason.InvalidName);

            await _teamRepository.AddAsync(team);
            return OperationResult<AddTeamFailureReason>.Ok();
        }

        public async Task<OperationResult<AddMemberFailureReason>> AddMemberAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
                return OperationResult<AddMemberFailureReason>.Fail(AddMemberFailureReason.TeamNotFound);
            if (!team.IsActive)
                return OperationResult<AddMemberFailureReason>.Fail(AddMemberFailureReason.TeamInactive);
           
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return OperationResult<AddMemberFailureReason>.Fail(AddMemberFailureReason.UserNotFound);
            if (!user.IsActive)
                return OperationResult<AddMemberFailureReason>.Fail(AddMemberFailureReason.UserInactive);

            if (team.UserIds.Contains(userId))
                return OperationResult<AddMemberFailureReason>.Fail(AddMemberFailureReason.UserAlreadyMember);

            team.UserIds.Add(userId);

            await _teamRepository.UpdateAsync(team);

            return OperationResult<AddMemberFailureReason>.Ok();
        }
        //outras funções 
    }
}
