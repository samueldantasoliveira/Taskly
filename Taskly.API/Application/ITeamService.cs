using Taskly.Domain.Entities;
using Taskly.Application.DTOs;

namespace Taskly.Application
{
    public interface ITeamService
    {

        public Task<StructuredOperationResult<TeamResponseDto>> AddTeamAsync(CreateTeamDto teamDto, Guid userId);
        public Task<StructuredOperationResult<TeamResponseDto>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto, Guid authenticatedUserId);
        public Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId);
        public Task<StructuredOperationResult<RemoveMemberResponseDto>> RemoveMemberAsync(Guid teamId, Guid userId, Guid authenticatedUserId);
        public Task<StructuredOperationResult> LeaveTeamAsync(Guid teamId, Guid authenticatedUserId);
        public Task<StructuredOperationResult<List<TeamResponseDto>>> GetUserTeamsAsync(Guid authenticatedUserId);
        public Task<StructuredOperationResult<TeamResponseDto>> GetByIdAsync(Guid teamId, Guid authenticatedUserId);
        public Task<StructuredOperationResult> DeleteTeamAsync(Guid teamId, Guid authenticatedUserId);
        
    }
}
