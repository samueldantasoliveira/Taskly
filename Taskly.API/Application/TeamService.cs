using Taskly.Domain.Entities;
using Taskly.Application.Results;
using Taskly.Application.DTOs;

namespace Taskly.Application
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserService _userService;

        public TeamService(ITeamRepository teamRepository, IUserService userService)
        {
            _teamRepository = teamRepository;
            _userService = userService;
        }

        public async Task<StructuredOperationResult<Team>> AddTeamAsync(CreateTeamDto teamDto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(teamDto.Name))
                return StructuredOperationResult<Team>.Fail(TeamErrors.InvalidName);
                
            var team = new Team(teamDto.Name, userId);

            await _teamRepository.AddAsync(team);
            return StructuredOperationResult<Team>.Ok(team);
        }

        public async Task<StructuredOperationResult<Team>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto, Guid authenticatedUserId)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
                return StructuredOperationResult<Team>.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<Team>.Fail(TeamErrors.NotOwner);

            team.Update(updateTeamDto.Name, updateTeamDto.IsActive);

            var updated = await _teamRepository.UpdateAsync(team);
            if (!updated)
                return StructuredOperationResult<Team>.Fail(TeamErrors.NotFound);
            return StructuredOperationResult<Team>.Ok(team);
        }

        public async Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.NotFound);
            if (!team.IsActive)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.Inactive);
           
            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(UserErrors.NotFound);

            if (team.UserIds.Contains(userId))
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.UserAlreadyMember);


            var added = await _teamRepository.AddMemberAsync(team.Id, user.Id);
            
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

        public async Task<StructuredOperationResult<RemoveMemberResponseDto>> RemoveMemberAsync(Guid teamId, Guid userId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);

            if (team == null)
                return StructuredOperationResult<RemoveMemberResponseDto>
                    .Fail(TeamErrors.NotFound);

            if (!team.IsActive)
                return StructuredOperationResult<RemoveMemberResponseDto>
                    .Fail(TeamErrors.Inactive);

            if (!team.UserIds.Contains(userId))
                return StructuredOperationResult<RemoveMemberResponseDto>
                    .Fail(TeamErrors.UserNotMember);

            var removed = await _teamRepository.RemoveMemberAsync(teamId, userId);

            if (!removed)
                return StructuredOperationResult<RemoveMemberResponseDto>
                    .Fail(TeamErrors.NotFound);

            return StructuredOperationResult<RemoveMemberResponseDto>.Ok(
                new RemoveMemberResponseDto
                {
                    TeamId = teamId,
                    UserId = userId,
                    RemovedAt = DateTime.UtcNow
                });

        }

        public async Task<Team?> GetByIdAsync(Guid teamId)
        {
            return await _teamRepository.GetByIdAsync(teamId);
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
