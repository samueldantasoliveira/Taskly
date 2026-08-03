using Taskly.Domain.Entities;
using Taskly.Application.DTOs;

namespace Taskly.Application
{
    public interface ITeamService
    {

        public Task<StructuredOperationResult<Team>> AddTeamAsync(CreateTeamDto teamDto, Guid userId);
        public Task<StructuredOperationResult<Team>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto, Guid authenticatedUserId);
        public Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId);
        public Task<StructuredOperationResult<RemoveMemberResponseDto>> RemoveMemberAsync(Guid teamId, Guid userId);
        public Task<Team?> GetByIdAsync(Guid teamId);
        public Task<StructuredOperationResult> DeleteTeamAsync(Guid teamId, Guid authenticatedUserId);
        
    }
}
