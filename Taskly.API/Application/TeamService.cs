using Taskly.Domain.Entities;
using Taskly.Application.Results;
using Taskly.Application.DTOs;

namespace Taskly.Application
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;

        public TeamService(ITeamRepository teamRepository, IUserRepository userService)
        {
            _teamRepository = teamRepository;
            _userRepository = userService;
        }

        public async Task<StructuredOperationResult<TeamResponseDto>> AddTeamAsync(CreateTeamDto teamDto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(teamDto.Name))
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.InvalidName);
                
            var team = new Team(teamDto.Name, userId);

            await _teamRepository.AddAsync(team);

            var teamResponseDto = new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                IsActive = team.IsActive,
                OwnerId = team.OwnerId,
                UserIds = team.UserIds.ToList()
            };
            return StructuredOperationResult<TeamResponseDto>.Ok(teamResponseDto);
        }

        public async Task<StructuredOperationResult<TeamResponseDto>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto, Guid authenticatedUserId)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotOwner);

            team.Update(updateTeamDto.Name, updateTeamDto.IsActive);

            var updated = await _teamRepository.UpdateAsync(team);
            if (!updated)
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotFound);

            var teamResponseDto = new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                IsActive = team.IsActive,
                OwnerId = team.OwnerId,
                UserIds = team.UserIds.ToList()
            };

            return StructuredOperationResult<TeamResponseDto>.Ok(teamResponseDto);
        }

        public async Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.NotFound);
            if (!team.IsActive)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.Inactive);
            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.NotOwner);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.UserNotFound);


            team.AddMember(user.Id);
            var added = await _teamRepository.UpdateAsync(team);
            
            if (!added)
                return StructuredOperationResult<AddMemberResponseDto>
                    .Fail(TeamErrors.NotFound);

            return StructuredOperationResult<AddMemberResponseDto>.Ok(new AddMemberResponseDto
            {
                UserId = user.Id,
                TeamId = team.Id,
                AddedAt = DateTime.UtcNow
            });
        }

        public async Task<StructuredOperationResult<RemoveMemberResponseDto>> RemoveMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);

            if (team == null)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.NotFound);

            if (!team.IsActive)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.Inactive);

            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.NotOwner);
                    
            team.RemoveMember(userId);
            var removed = await _teamRepository.UpdateAsync(team);

            if (!removed)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.NotFound);

            return StructuredOperationResult<RemoveMemberResponseDto>.Ok(
                new RemoveMemberResponseDto
                {
                    TeamId = teamId,
                    UserId = userId,
                    RemovedAt = DateTime.UtcNow
                });

        }

        public async Task<TeamResponseDto?> GetByIdAsync(Guid teamId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if(team == null)
                return null;
                
            var teamResponseDto = new TeamResponseDto
            {
                Id = team.Id,
                Name = team.Name,
                IsActive = team.IsActive,
                OwnerId = team.OwnerId,
                UserIds = team.UserIds.ToList()
            };

            return teamResponseDto;
        }

        public async Task<StructuredOperationResult> DeleteTeamAsync(Guid teamId, Guid authenticatedUserId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
                return StructuredOperationResult.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult.Fail(TeamErrors.NotOwner);

            var deleted = await _teamRepository.DeleteAsync(teamId);
            if (!deleted)
                return StructuredOperationResult.Fail(TeamErrors.NotFound);
            
            return StructuredOperationResult.Ok();
        }

    }
}
