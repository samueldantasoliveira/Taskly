using Rivulus.Domain.Entities;
using Rivulus.Application.Results;
using Rivulus.Application.DTOs;
using System.Security.Cryptography;

namespace Rivulus.Application
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;

        private readonly IUserResponsibilities _responsibilities;
        private readonly ITeamInvitationRepository? _invitationRepository;
        public TeamService(ITeamRepository teamRepository, IUserRepository userService, IUserResponsibilities responsibilities, ITeamInvitationRepository? invitationRepository = null)
        {
            _teamRepository = teamRepository;
            _userRepository = userService;
            _responsibilities = responsibilities;
            _invitationRepository = invitationRepository;
        }

        public async Task<StructuredOperationResult<TeamResponseDto>> AddTeamAsync(CreateTeamDto teamDto, Guid userId, CancellationToken cancellationToken = default)
        {
            if (!IsValidName(teamDto.Name))
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.InvalidName);

            var team = new Team(teamDto.Name, userId);

            await _teamRepository.AddAsync(team, cancellationToken);

            var teamResponseDto = new TeamResponseDto
            {
                Id = team.Id,
                Version = team.Version,
                Name = team.Name,
                IsActive = team.IsActive,
                OwnerId = team.OwnerId,
                UserIds = team.UserIds.ToList()
            };
            return StructuredOperationResult<TeamResponseDto>.Ok(teamResponseDto);
        }

        public async Task<StructuredOperationResult<TeamResponseDto>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(id, cancellationToken);
            if (team == null)
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotOwner);

            ConcurrencyConflictException.Check(updateTeamDto.Version, team.Version);
            if (updateTeamDto.Name != null && !IsValidName(updateTeamDto.Name))
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.InvalidName);
            if (updateTeamDto.OwnerId is Guid ownerId)
            {
                if (!team.UserIds.Contains(ownerId))
                    return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.UserNotMember);
                if (await _userRepository.GetByIdAsync(ownerId, cancellationToken) == null)
                    return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.UserNotFound);
                team.TransferOwnership(ownerId);
            }
            team.Update(updateTeamDto.Name, updateTeamDto.IsActive);

            var updated = await _teamRepository.UpdateAsync(team, cancellationToken);
            if (!updated)
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotFound);

            var teamResponseDto = new TeamResponseDto
            {
                Id = team.Id,
                Version = team.Version,
                Name = team.Name,
                IsActive = team.IsActive,
                OwnerId = team.OwnerId,
                UserIds = team.UserIds.ToList()
            };

            return StructuredOperationResult<TeamResponseDto>.Ok(teamResponseDto);
        }

        public async Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.NotFound);
            if (!team.IsActive)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.Inactive);
            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.NotOwner);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
                return StructuredOperationResult<AddMemberResponseDto>.Fail(TeamErrors.UserNotFound);


            team.AddMember(user.Id);
            var added = await _teamRepository.UpdateAsync(team, cancellationToken);

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

        public async Task<StructuredOperationResult<RemoveMemberResponseDto>> RemoveMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);

            if (team == null)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.NotFound);

            if (!team.IsActive)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.Inactive);

            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.NotOwner);

            team.RemoveMember(userId);
            var removed = await _teamRepository.UpdateAsync(team, cancellationToken);

            if (!removed)
                return StructuredOperationResult<RemoveMemberResponseDto>.Fail(TeamErrors.NotFound);

            await _responsibilities.ReleaseActiveTasksAsync(teamId, userId, cancellationToken);
            return StructuredOperationResult<RemoveMemberResponseDto>.Ok(
                new RemoveMemberResponseDto
                {
                    TeamId = teamId,
                    UserId = userId,
                    RemovedAt = DateTime.UtcNow
                });

        }

        public async Task<StructuredOperationResult> LeaveTeamAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);

            if (team == null)
                return StructuredOperationResult.Fail(TeamErrors.NotFound);

            if (!team.IsActive)
                return StructuredOperationResult.Fail(TeamErrors.Inactive);
            team.RemoveMember(authenticatedUserId);

             var removed = await _teamRepository.UpdateAsync(team, cancellationToken);

            if (!removed)
                return StructuredOperationResult.Fail(TeamErrors.NotFound);

            await _responsibilities.ReleaseActiveTasksAsync(teamId, authenticatedUserId, cancellationToken);
            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult<TeamResponseDto>> GetByIdAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if(team == null)
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotFound);

            if (!team.UserIds.Contains(authenticatedUserId))
                return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotAuthorized);

            var teamResponseDto = new TeamResponseDto
            {
                Id = team.Id,
                Version = team.Version,
                Name = team.Name,
                IsActive = team.IsActive,
                OwnerId = team.OwnerId,
                UserIds = team.UserIds.ToList()
            };

            return StructuredOperationResult<TeamResponseDto>.Ok(teamResponseDto);
        }

        public async Task<StructuredOperationResult<List<TeamMemberResponseDto>>> GetMembersAsync(
            Guid teamId,
            Guid authenticatedUserId,
            CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if (team == null)
            {
                return StructuredOperationResult<List<TeamMemberResponseDto>>
                    .Fail(TeamErrors.NotFound);
            }

            if (!team.UserIds.Contains(authenticatedUserId))
            {
                return StructuredOperationResult<List<TeamMemberResponseDto>>
                    .Fail(TeamErrors.NotAuthorized);
            }

            var users = await _userRepository.GetByIdsAsync(
                team.UserIds,
                cancellationToken);

            var members = users
                .Select(user => new TeamMemberResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    AvatarKey = user.AvatarKey,
                    IsOwner = user.Id == team.OwnerId
                })
                .ToList();

            return StructuredOperationResult<List<TeamMemberResponseDto>>.Ok(members);
        }

        public async Task<StructuredOperationResult> DeleteTeamAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if (team == null)
                return StructuredOperationResult.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId)
                return StructuredOperationResult.Fail(TeamErrors.NotOwner);

            var deleted = await _teamRepository.DeleteAsync(teamId, cancellationToken);
            if (!deleted)
                return StructuredOperationResult.Fail(TeamErrors.NotFound);

            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult<List<TeamResponseDto>>> GetUserTeamsAsync(Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(authenticatedUserId, cancellationToken);
            if(user == null)
                return StructuredOperationResult<List<TeamResponseDto>>.Fail(TeamErrors.UserNotFound);

            var teams = await _teamRepository.GetUserTeamsAsync(authenticatedUserId, cancellationToken);

            var response = teams
                .Select(team => new TeamResponseDto
                {
                    Id = team.Id,
                    Version = team.Version,
                    Name = team.Name,
                    IsActive = team.IsActive,
                    OwnerId = team.OwnerId,
                    UserIds = team.UserIds.ToList()
                })
                .ToList();

            return StructuredOperationResult<List<TeamResponseDto>>.Ok(response);
        }

        public async Task<StructuredOperationResult<TeamInvitationResponseDto>> CreateInvitationAsync(Guid teamId, CreateTeamInvitationDto dto, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if (team == null) return StructuredOperationResult<TeamInvitationResponseDto>.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId) return StructuredOperationResult<TeamInvitationResponseDto>.Fail(TeamErrors.NotOwner);
            if (!team.IsActive) return StructuredOperationResult<TeamInvitationResponseDto>.Fail(TeamErrors.Inactive);
            var email = dto.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || !User.IsValidEmail(email)) return StructuredOperationResult<TeamInvitationResponseDto>.Fail(TeamErrors.InvalidInvitationEmail);
            var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (existingUser != null && team.UserIds.Contains(existingUser.Id)) return StructuredOperationResult<TeamInvitationResponseDto>.Fail(TeamErrors.UserAlreadyMember);
            if (_invitationRepository == null || await _invitationRepository.HasPendingAsync(teamId, email, cancellationToken)) return StructuredOperationResult<TeamInvitationResponseDto>.Fail(TeamErrors.InvitationAlreadyPending);
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            var invitation = new TeamInvitation(teamId, email, HashToken(token), DateTime.UtcNow.AddDays(7));
            await _invitationRepository.AddAsync(invitation, cancellationToken);
            return StructuredOperationResult<TeamInvitationResponseDto>.Ok(ToInvitationDto(invitation, team.Name, token));
        }

        public async Task<StructuredOperationResult<List<TeamInvitationResponseDto>>> GetInvitationsAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if (team == null) return StructuredOperationResult<List<TeamInvitationResponseDto>>.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId) return StructuredOperationResult<List<TeamInvitationResponseDto>>.Fail(TeamErrors.NotOwner);
            var values = _invitationRepository == null ? [] : await _invitationRepository.GetPendingByTeamIdAsync(teamId, cancellationToken);
            return StructuredOperationResult<List<TeamInvitationResponseDto>>.Ok(values.Select(x => ToInvitationDto(x, team.Name)).ToList());
        }

        public async Task<StructuredOperationResult> RevokeInvitationAsync(Guid teamId, Guid invitationId, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
            if (team == null) return StructuredOperationResult.Fail(TeamErrors.NotFound);
            if (team.OwnerId != authenticatedUserId) return StructuredOperationResult.Fail(TeamErrors.NotOwner);
            var invitation = _invitationRepository == null ? null : await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
            if (invitation == null || invitation.TeamId != teamId || !invitation.IsPending) return StructuredOperationResult.Fail(TeamErrors.InvitationNotFound);
            invitation.Revoke(); await _invitationRepository!.UpdateAsync(invitation, cancellationToken);
            return StructuredOperationResult.Ok();
        }

        public async Task<StructuredOperationResult<TeamResponseDto>> AcceptInvitationAsync(string token, Guid authenticatedUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token) || _invitationRepository == null) return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.InvitationNotFound);
            var invitation = await _invitationRepository.GetByTokenHashAsync(HashToken(token), cancellationToken);
            if (invitation == null) return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.InvitationNotFound);
            if (!invitation.IsPending) return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.InvitationExpired);
            var user = await _userRepository.GetByIdAsync(authenticatedUserId, cancellationToken);
            if (user == null) return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.UserNotFound);
            if (!string.Equals(user.Email, invitation.Email, StringComparison.OrdinalIgnoreCase)) return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.InvitationEmailMismatch);
            var team = await _teamRepository.GetByIdAsync(invitation.TeamId, cancellationToken);
            if (team == null) return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.NotFound);
            if (!team.IsActive) return StructuredOperationResult<TeamResponseDto>.Fail(TeamErrors.Inactive);
            if (!team.UserIds.Contains(user.Id)) { team.AddMember(user.Id); await _teamRepository.UpdateAsync(team, cancellationToken); }
            invitation.Accept(); await _invitationRepository.UpdateAsync(invitation, cancellationToken);
            return StructuredOperationResult<TeamResponseDto>.Ok(new TeamResponseDto { Id = team.Id, Version = team.Version, Name = team.Name, IsActive = team.IsActive, OwnerId = team.OwnerId, UserIds = team.UserIds.ToList() });
        }

        private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
        private static TeamInvitationResponseDto ToInvitationDto(TeamInvitation value, string teamName, string? token = null) => new() { Id = value.Id, TeamId = value.TeamId, TeamName = teamName, Email = value.Email, Token = token, ExpiresAt = value.ExpiresAt, CreatedAt = value.CreatedAt };

        private static bool IsValidName(string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length is >= 2 and <= 100;

    }
}
