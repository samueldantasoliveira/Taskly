using Rivulus.Domain.Entities;
using Rivulus.Application.DTOs;

namespace Rivulus.Application
{
    public interface ITeamService
    {

        public Task<StructuredOperationResult<TeamResponseDto>> AddTeamAsync(CreateTeamDto teamDto, Guid userId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult<TeamResponseDto>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult<RemoveMemberResponseDto>> RemoveMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult> LeaveTeamAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult<List<TeamResponseDto>>> GetUserTeamsAsync(Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult<TeamResponseDto>> GetByIdAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult<List<TeamMemberResponseDto>>> GetMembersAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult> DeleteTeamAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        Task<StructuredOperationResult<TeamInvitationResponseDto>> CreateInvitationAsync(Guid teamId, CreateTeamInvitationDto dto, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        Task<StructuredOperationResult<List<TeamInvitationResponseDto>>> GetInvitationsAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        Task<StructuredOperationResult> RevokeInvitationAsync(Guid teamId, Guid invitationId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        Task<StructuredOperationResult<TeamResponseDto>> AcceptInvitationAsync(string token, Guid authenticatedUserId, CancellationToken cancellationToken = default);
    }
}
