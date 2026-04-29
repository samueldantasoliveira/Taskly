using Taskly.Application.DTOs;
using Taskly.Domain.Entities;

namespace Taskly.Application
{
    public interface IProjectService
    {
        public Task<StructuredOperationResult<Project>> AddProjectAsync(CreateProjectDto projectDto, Guid ownerId);

        public Task<Project?> GetByIdAsync(Guid id);
    }
}
