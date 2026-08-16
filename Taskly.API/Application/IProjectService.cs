using Taskly.Application.DTOs;
using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface IProjectService
    {
        public Task<StructuredOperationResult<ProjectResponseDto>> AddProjectAsync(CreateProjectDto projectDto, Guid ownerId);

        public Task<ProjectResponseDto?> GetByIdAsync(Guid id);

        public Task<StructuredOperationResult<ProjectResponseDto>> UpdateProjectAsync(Guid id, UpdateProjectDto updateProjectDto, Guid authenticatedUserId);
        public Task<StructuredOperationResult> DeleteProjectAsync(Guid id, Guid authenticatedUserId);    
    }
}
