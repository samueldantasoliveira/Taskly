using Taskly.Domain.Entities;
using Taskly.Application.DTOs;

namespace Taskly.Application
{
    public interface ITeamService
    {

        public Task<StructuredOperationResult<Team>> AddTeamAsync(CreateTeamDto teamDto);

        public Task<StructuredOperationResult<AddMemberResponseDto>> AddMemberAsync(Guid teamId, Guid userId);
        public Task<Team?> GetByIdAsync(Guid teamId);
        
    }
}
