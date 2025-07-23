using Taskly.Domain.Entities;
using Taskly.Application.Results;
using Taskly.Infrastructure;
using Taskly.Application.DTOs;

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

        public async Task<StructuredOperationResult<Team>> AddTeamAsync(CreateTeamDto teamDto)
        {
            var team = new Team(teamDto.Name);
            if (String.IsNullOrWhiteSpace(team.Name))
                return StructuredOperationResult<Team>.Fail(Error.FromEnum(AddTeamFailureReason.InvalidName));

            await _teamRepository.AddAsync(team);
            return StructuredOperationResult<Team>.Ok(team);
        }

        public async Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(Error.FromEnum(AddMemberFailureReason.TeamNotFound));
            if (!team.IsActive)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(Error.FromEnum(AddMemberFailureReason.TeamInactive));
           
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(Error.FromEnum(AddMemberFailureReason.UserNotFound));
            if (!user.IsActive)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(Error.FromEnum(AddMemberFailureReason.UserInactive));

            if (team.UserIds.Contains(userId))
                return StructuredOperationResult<AddMemberResponseDto>.Fail(Error.FromEnum(AddMemberFailureReason.UserAlreadyMember));

            team.UserIds.Add(userId);

            await _teamRepository.UpdateAsync(team);

            return StructuredOperationResult<AddMemberResponseDto>.Ok(new AddMemberResponseDto
            {
                UserId = user.Id,
                TeamId = team.Id,
                AddedAt = DateTime.UtcNow
            });
        }
        
    }
}
