using Taskly.Application.DTOs;
using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface IProjectService
    {
        public Task<StructuredOperationResult<List<ProjectResponseDto>>> GetTeamProjectsAsync(Guid teamId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult<ProjectResponseDto>> AddProjectAsync(CreateProjectDto projectDto, Guid ownerId, CancellationToken cancellationToken = default);

        public Task<StructuredOperationResult<ProjectResponseDto>> GetByIdAsync(Guid id, Guid authenticatedUserId, CancellationToken cancellationToken = default);

        public Task<StructuredOperationResult<ProjectResponseDto>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto, Guid authenticatedUserId, CancellationToken cancellationToken = default);
        public Task<StructuredOperationResult> DeleteProjectAsync(Guid id, Guid authenticatedUserId, CancellationToken cancellationToken = default);    
    }
}
